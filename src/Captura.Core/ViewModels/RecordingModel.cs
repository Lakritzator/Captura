using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Captura.Base;
using Captura.Base.Audio;
using Captura.Base.Images;
using Captura.Base.Notification;
using Captura.Base.Recent;
using Captura.Base.Services;
using Captura.Base.Video;
using Captura.Core.Models;
using Captura.Core.Models.Notifications;
using Captura.Core.Models.Recents;
using Captura.FFmpeg;
using Captura.FFmpeg.Audio;
using Captura.FFmpeg.Video;
using Captura.Loc;
using Captura.MouseKeyHook;
using Captura.WebCam;
using DesktopDuplication;
using Microsoft.Win32;
using Screna;
using Screna.Gif;
using Screna.ImageProviders;
using Screna.Overlays;
using Screna.Recorder;
using Screna.VideoItems;
using Screna.VideoSourceProviders;

namespace Captura.Core.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RecordingModel : ViewModelBase, IDisposable
    {
        #region Fields
        IRecorder _recorder;
        string _currentFileName;
        bool _isVideo;

        readonly SynchronizationContext _syncContext = SynchronizationContext.Current;

        readonly ISystemTray _systemTray;
        readonly IRegionProvider _regionProvider;
        readonly WebCamOverlay _webcamOverlay;
        readonly IMainWindow _mainWindow;
        readonly IPreviewWindow _previewWindow;
        readonly IWebCamProvider _webCamProvider;
        readonly IAudioPlayer _audioPlayer;
        readonly TimerModel _timerModel;

        readonly KeymapViewModel _keymap;

        readonly VideoSourcesViewModel _videoSourcesViewModel;
        readonly VideoWritersViewModel _videoWritersViewModel;
        readonly AudioSource _audioSource;
        readonly IRecentList _recentList;
        #endregion

        public RecordingModel(Settings.Settings settings,
            LanguageManager languageManager,
            ISystemTray systemTray,
            IRegionProvider regionProvider,
            WebCamOverlay webcamOverlay,
            IMainWindow mainWindow,
            IPreviewWindow previewWindow,
            VideoSourcesViewModel videoSourcesViewModel,
            VideoWritersViewModel videoWritersViewModel,
            AudioSource audioSource,
            IWebCamProvider webCamProvider,
            KeymapViewModel keymap,
            IAudioPlayer audioPlayer,
            IRecentList recentList,
            TimerModel timerModel) : base(settings, languageManager)
        {
            _systemTray = systemTray;
            _regionProvider = regionProvider;
            _webcamOverlay = webcamOverlay;
            _mainWindow = mainWindow;
            _previewWindow = previewWindow;
            _videoSourcesViewModel = videoSourcesViewModel;
            _videoWritersViewModel = videoWritersViewModel;
            _audioSource = audioSource;
            _webCamProvider = webCamProvider;
            _keymap = keymap;
            _audioPlayer = audioPlayer;
            _recentList = recentList;
            _timerModel = timerModel;

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

            timerModel.CountdownElapsed += InternalStartRecording;
            timerModel.DurationElapsed += async () =>
            {
                if (_syncContext != null)
                    _syncContext.Post(async state => await StopRecording(), null);
                else await StopRecording();
            };
        }

        RecorderState _recorderState = RecorderState.NotRecording;

        public RecorderState RecorderState
        {
            get => _recorderState;
            private set
            {
                if (_recorderState == value)
                    return;

                _recorderState = value;

                OnPropertyChanged();
            }
        }

        public async void OnRecordExecute()
        {
            if (RecorderState == RecorderState.NotRecording)
            {
                _audioPlayer.Play(SoundKind.Start);

                StartRecording();
            }
            else
            {
                _audioPlayer.Play(SoundKind.Stop);

                await StopRecording();
            }
        }

        void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend && RecorderState == RecorderState.Recording)
            {
                OnPauseExecute();
            }
        }

        public void Dispose()
        {
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
        }

        INotification _pauseNotification;

        public void OnPauseExecute()
        {
            _audioPlayer.Play(SoundKind.Pause);

            // Resume
            if (RecorderState == RecorderState.Paused)
            {
                _systemTray.HideNotification();

                _recorder.Start();
                _timerModel.Resume();

                RecorderState = RecorderState.Recording;

                _pauseNotification?.Remove();
            }
            else // Pause
            {
                _recorder.Stop();
                _timerModel.Pause();

                RecorderState = RecorderState.Paused;

                _pauseNotification = new TextNotification(Loc.Paused, OnPauseExecute);
                _systemTray.ShowNotification(_pauseNotification);
            }
        }

        public bool StartRecording(string fileName = null)
        {
            Settings.EnsureOutPath();

            _isVideo = !(_videoSourcesViewModel.SelectedVideoSourceKind is NoVideoSourceProvider);

            var extension = _videoWritersViewModel.SelectedVideoWriter.Extension;

            if (_videoSourcesViewModel.SelectedVideoSourceKind?.Source is NoVideoItem x)
                extension = x.Extension;

            _currentFileName = Settings.GetFileName(extension, fileName);

            if (_videoWritersViewModel.SelectedVideoWriterKind is FFmpegWriterProvider ||
                _videoWritersViewModel.SelectedVideoWriterKind is StreamingWriterProvider ||
                (_videoSourcesViewModel.SelectedVideoSourceKind is NoVideoSourceProvider noVideoSourceProvider
                 && noVideoSourceProvider.Source is FFmpegAudioItem))
            {
                if (!FFmpegService.FFmpegExists)
                {
                    ServiceProvider.MessageProvider.ShowFFmpegUnavailable();

                    return false;
                }
            }

            if (_videoWritersViewModel.SelectedVideoWriterKind is GifWriterProvider)
            {
                if (Settings.Audio.Enabled)
                {
                    if (!ServiceProvider.MessageProvider.ShowYesNo("Audio won't be included in the recording.\nDo you want to record?", "Gif does not support Audio"))
                    {
                        return false;
                    }
                }
            }

            IImageProvider imgProvider;

            try
            {
                imgProvider = GetImageProvider();
            }
            catch (NotSupportedException e) when (_videoSourcesViewModel.SelectedVideoSourceKind is DesktopDuplicationSourceProvider)
            {
                var yes = ServiceProvider.MessageProvider.ShowYesNo($"{e.Message}\n\nDo you want to turn off Desktop Duplication.", Loc.ErrorOccurred);

                if (yes)
                    _videoSourcesViewModel.SetDefaultSource();

                return false;
            }
            catch (WindowClosedException)
            {
                ServiceProvider.MessageProvider.ShowError("Selected Window has been Closed.", "Window Closed");

                return false;
            }
            catch (Exception e)
            {
                ServiceProvider.MessageProvider.ShowException(e, e.Message);

                return false;
            }

            IAudioProvider audioProvider = null;

            try
            {
                if (Settings.Audio.Enabled && !Settings.Audio.SeparateFilePerSource)
                {
                    audioProvider = _audioSource.GetMixedAudioProvider();
                }
            }
            catch (Exception e)
            {
                ServiceProvider.MessageProvider.ShowException(e, e.Message);

                imgProvider?.Dispose();

                return false;
            }

            IVideoFileWriter videoEncoder;

            try
            {
                videoEncoder = GetVideoFileWriterWithPreview(imgProvider, audioProvider);
            }
            catch (Exception e)
            {
                ServiceProvider.MessageProvider.ShowException(e, e.Message);

                imgProvider?.Dispose();
                audioProvider?.Dispose();

                return false;
            }

            switch (videoEncoder)
            {
                case GifWriter gif when Settings.Gif.VariableFrameRate:
                    _recorder = new VfrGifRecorder(gif, imgProvider);
                    break;

                case WithPreviewWriter previewWriter when previewWriter.OriginalWriter is GifWriter gif && Settings.Gif.VariableFrameRate:
                    _recorder = new VfrGifRecorder(gif, imgProvider);
                    break;

                default:
                    if (_isVideo)
                    {
                        _recorder = new Recorder(videoEncoder, imgProvider, Settings.Video.FrameRate, audioProvider);
                    }
                    else if (_videoSourcesViewModel.SelectedVideoSourceKind?.Source is NoVideoItem audioWriter)
                    {
                        IRecorder GetAudioRecorder(IAudioProvider audioProviderToGet, string audioFileName = null)
                        {
                            return new Recorder(
                                audioWriter.GetAudioFileWriter(audioFileName ?? _currentFileName, audioProviderToGet?.WaveFormat, Settings.Audio.Quality), audioProviderToGet);
                        }

                        if (!Settings.Audio.SeparateFilePerSource)
                        {
                            _recorder = GetAudioRecorder(audioProvider);
                        }
                        else
                        {
                            string GetAudioFileName(int index)
                            {
                                return Path.ChangeExtension(_currentFileName,
                                    $".{index}{Path.GetExtension(_currentFileName)}");
                            }

                            var audioProviders = _audioSource.GetMultipleAudioProviders();

                            if (audioProviders.Length > 0)
                            {
                                var recorders = audioProviders
                                    .Select((audioProviderToGet, index) => GetAudioRecorder(audioProviderToGet, GetAudioFileName(index)))
                                    .ToArray();

                                _recorder = new MultiRecorder(recorders);

                                // Set to first file
                                _currentFileName = GetAudioFileName(0);
                            }
                            else
                            {
                                ServiceProvider.MessageProvider.ShowError("No Audio Sources selected");

                                return false;
                            }
                        }
                    }
                    break;
            }

            // Separate file for webcam
            if (_isVideo && _webCamProvider.SelectedCam != WebCamItem.NoWebCam && Settings.WebCamOverlay.SeparateFile)
            {
                SeparateFileForWebCam();
            }

            // Separate file for every audio source
            if (_isVideo && Settings.Audio.Enabled && Settings.Audio.SeparateFilePerSource)
            {
                SeparateFileForEveryAudioSource();
            }

            if (_videoSourcesViewModel.SelectedVideoSourceKind is RegionSourceProvider)
                _regionProvider.Lock();

            _systemTray.HideNotification();

            if (Settings.Ui.MinimizeOnStart)
                _mainWindow.IsMinimized = true;

            RecorderState = RecorderState.Recording;

            _recorder.ErrorOccurred += OnErrorOccured;

            if (Settings.PreStartCountdown == 0)
            {
                InternalStartRecording();
            }

            _timerModel.Start();

            return true;
        }

        void SeparateFileForWebCam()
        {
            var webCamImgProvider = new WebCamImageProvider(_webCamProvider);

            var webcamFileName = Path.ChangeExtension(_currentFileName, $".webcam{Path.GetExtension(_currentFileName)}");

            var webcamVideoWriter = GetVideoFileWriter(webCamImgProvider, null, webcamFileName);

            var webcamRecorder = new Recorder(webcamVideoWriter, webCamImgProvider, Settings.Video.FrameRate);

            _recorder = new MultiRecorder(_recorder, webcamRecorder);
        }

        void SeparateFileForEveryAudioSource()
        {
            var audioWriter = WaveItem.Instance;

            IRecorder GetAudioRecorder(IAudioProvider audioProvider, string audioFileName = null)
            {
                return new Recorder(
                    audioWriter.GetAudioFileWriter(audioFileName ?? _currentFileName, audioProvider?.WaveFormat,
                        Settings.Audio.Quality), audioProvider);
            }

            string GetAudioFileName(int index)
            {
                return Path.ChangeExtension(_currentFileName, $".{index}.wav");
            }

            var audioProviders = _audioSource.GetMultipleAudioProviders();

            if (audioProviders.Length > 0)
            {
                var recorders = audioProviders
                    .Select((audioProvider, index) => GetAudioRecorder(audioProvider, GetAudioFileName(index)))
                    .Concat(new[] {_recorder})
                    .ToArray();

                _recorder = new MultiRecorder(recorders);
            }
        }

        void InternalStartRecording()
        {
            _recorder.Start();
        }

        void OnErrorOccured(Exception exception)
        {
            void Do()
            {
                var cancelled = exception is WindowClosedException;

                AfterRecording();

                if (!cancelled)
                    ServiceProvider.MessageProvider.ShowException(exception, exception.Message);

                if (cancelled)
                {
                    _videoSourcesViewModel.SetDefaultSource();
                }
            }

            if (_syncContext != null)
                _syncContext.Post(state => Do(), null);
            else Do();
        }

        void AfterRecording()
        {
            _pauseNotification?.Remove();

            RecorderState = RecorderState.NotRecording;

            _recorder.ErrorOccurred -= OnErrorOccured;
            _recorder = null;

            _timerModel.Stop();

            if (Settings.Ui.MinimizeOnStart)
                _mainWindow.IsMinimized = false;

            if (_videoSourcesViewModel.SelectedVideoSourceKind is RegionSourceProvider)
                _regionProvider.Release();
        }

        IVideoFileWriter GetVideoFileWriterWithPreview(IImageProvider imgProvider, IAudioProvider audioProvider)
        {
            if (_videoSourcesViewModel.SelectedVideoSourceKind is NoVideoSourceProvider)
                return null;

            _previewWindow.Init(imgProvider.Width, imgProvider.Height);

            return new WithPreviewWriter(GetVideoFileWriter(imgProvider, audioProvider), _previewWindow);
        }

        IVideoFileWriter GetVideoFileWriter(IImageProvider imgProvider, IAudioProvider audioProvider, string fileName = null)
        {
            if (_videoSourcesViewModel.SelectedVideoSourceKind is NoVideoSourceProvider)
                return null;

            return _videoWritersViewModel.SelectedVideoWriter.GetVideoFileWriter(new VideoWriterArgs
            {
                FileName = fileName ?? _currentFileName,
                FrameRate = Settings.Video.FrameRate,
                VideoQuality = Settings.Video.Quality,
                ImageProvider = imgProvider,
                AudioQuality = Settings.Audio.Quality,
                AudioProvider = audioProvider
            });
        }

        IEnumerable<IOverlay> GetOverlays()
        {
            yield return new CensorOverlay(Settings.Censored);

            if (!Settings.WebCamOverlay.SeparateFile)
            {
                yield return _webcamOverlay;
            }

            yield return new MousePointerOverlay(Settings.MousePointerOverlay);

            yield return new MouseKeyHook.MouseKeyHook(Settings.Clicks,
                Settings.Keystrokes,
                _keymap,
                _currentFileName,
                () => _timerModel.TimeSpan);

            yield return new ElapsedOverlay(Settings.Elapsed, () => _timerModel.TimeSpan);

            // Text Overlays
            foreach (var overlay in Settings.TextOverlays)
            {
                yield return new CustomOverlay(overlay);
            }

            // Image Overlays
            foreach (var overlay in Settings.ImageOverlays.Where(customImageOverlaySettings => customImageOverlaySettings.Display))
            {
                IOverlay imgOverlay = null;

                try
                {
                    imgOverlay = new CustomImageOverlay(overlay);
                }
                catch
                {
                    // Ignore Errors like Image not found, Invalid Image
                }

                if (imgOverlay != null)
                    yield return imgOverlay;
            }
        }

        IImageProvider GetImageProvider()
        {
            Func<Point, Point> transform = point => point;

            var imageProvider = _videoSourcesViewModel
                .SelectedVideoSourceKind
                ?.Source
                ?.GetImageProvider(Settings.IncludeCursor, out transform);

            return imageProvider == null
                ? null
                : new OverlayedImageProvider(imageProvider, transform, GetOverlays().ToArray());
        }

        readonly object _stopRecTaskLock = new object();
        readonly List<Task> _stopRecTasks = new List<Task>();

        public int RunningStopRecordingCount
        {
            get
            {
                lock (_stopRecTaskLock)
                {
                    return _stopRecTasks.Count(task => !task.IsCompleted);
                }
            }
        }

        public async Task StopRecording()
        {
            FileRecentItem savingRecentItem = null;
            FileSaveNotification notification = null;

            // Reference current file name
            var fileName = _currentFileName;

            // Assume saving to file only when extension is present
            if (!_timerModel.Waiting && !string.IsNullOrWhiteSpace(_videoWritersViewModel.SelectedVideoWriter.Extension))
            {
                savingRecentItem = new FileRecentItem(_currentFileName, _isVideo ? RecentFileType.Video : RecentFileType.Audio, true);
                _recentList.Add(savingRecentItem);

                notification = new FileSaveNotification(savingRecentItem);

                notification.OnDelete += () => savingRecentItem.RemoveCommand.ExecuteIfCan();

                _systemTray.ShowNotification(notification);
            }

            // Reference Recorder as it will be set to null
            var rec = _recorder;

            var task = Task.Run(() => rec.Dispose());

            lock (_stopRecTaskLock)
            {
                _stopRecTasks.Add(task);
            }

            AfterRecording();

            var wasWaiting = _timerModel.Waiting;
            _timerModel.Waiting = false;

            try
            {
                // Ensure saved
                await task;

                lock (_stopRecTaskLock)
                {
                    _stopRecTasks.Remove(task);
                }
            }
            catch (Exception e)
            {
                ServiceProvider.MessageProvider.ShowException(e, "Error occurred when stopping recording.\nThis might sometimes occur if you stop recording just as soon as you start it.");

                return;
            }

            if (wasWaiting)
            {
                try
                {
                    File.Delete(fileName);
                }
                catch
                {
                    // Ignore Errors
                }
            }

            if (savingRecentItem != null)
            {
                AfterSave(savingRecentItem, notification);
            }
        }

        void AfterSave(FileRecentItem savingRecentItem, FileSaveNotification notification)
        {
            savingRecentItem.Saved();
        
            if (Settings.CopyOutPathToClipboard)
                savingRecentItem.FileName.WriteToClipboard();

            notification.Saved();
        }

        public bool CanExit()
        {
            if (RecorderState == RecorderState.Recording)
            {
                if (!ServiceProvider.MessageProvider.ShowYesNo(
                    "A Recording is in progress. Are you sure you want to exit?", "Confirm Exit"))
                    return false;
            }
            else if (RunningStopRecordingCount > 0)
            {
                if (!ServiceProvider.MessageProvider.ShowYesNo(
                    "Some Recordings have not finished writing to disk. Are you sure you want to exit?", "Confirm Exit"))
                    return false;
            }

            return true;
        }
    }
}