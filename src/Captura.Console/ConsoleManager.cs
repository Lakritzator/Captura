using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Captura.Base.Audio;
using Captura.Base.Services;
using Captura.Base.Video;
using Captura.CmdOptions;
using Captura.Core.Models;
using Captura.Core.Settings;
using Captura.Core.ViewModels;
using Captura.FFmpeg;
using Captura.FFmpeg.Video;
using Captura.SharpAvi;
using Screna.Gif;
using static System.Console;

namespace Captura
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ConsoleManager : IDisposable
    {
        private readonly Settings _settings;
        private readonly MainModel _mainModel;
        private readonly RecordingModel _recordingModel;
        private readonly ScreenShotModel _screenShotModel;
        private readonly VideoSourcesViewModel _videoSourcesViewModel;
        private readonly IEnumerable<IVideoSourceProvider> _videoSourceProviders;
        private readonly IWebCamProvider _webCamProvider;
        private readonly VideoWritersViewModel _videoWritersViewModel;
        private readonly IPlatformServices _platformServices;

        public ConsoleManager(Settings settings,
            RecordingModel recordingModel,
            MainModel mainModel,
            ScreenShotModel screenShotModel,
            VideoSourcesViewModel videoSourcesViewModel,
            IEnumerable<IVideoSourceProvider> videoSourceProviders,
            IWebCamProvider webCamProvider,
            VideoWritersViewModel videoWritersViewModel,
            IPlatformServices platformServices)
        {
            _settings = settings;
            _recordingModel = recordingModel;
            _mainModel = mainModel;
            _screenShotModel = screenShotModel;
            _videoSourcesViewModel = videoSourcesViewModel;
            _videoSourceProviders = videoSourceProviders;
            _webCamProvider = webCamProvider;
            _videoWritersViewModel = videoWritersViewModel;
            _platformServices = platformServices;

            // Hide on Full Screen Screenshot doesn't work on Console
            settings.Ui.HideOnFullScreenShot = false;
        }

        public void Dispose()
        {
            _mainModel.Dispose();
        }

        public void CopySettings()
        {
            // Load settings dummy
            var dummySettings = new Settings();
            dummySettings.Load();

            _settings.WebCamOverlay = dummySettings.WebCamOverlay;
            _settings.MousePointerOverlay = dummySettings.MousePointerOverlay;
            _settings.Clicks = dummySettings.Clicks;
            _settings.Keystrokes = dummySettings.Keystrokes;
            _settings.Elapsed = dummySettings.Elapsed;

            // FFmpeg Path
            _settings.FFmpeg.FolderPath = dummySettings.FFmpeg.FolderPath;

            foreach (var overlay in dummySettings.Censored)
            {
                _settings.Censored.Add(overlay);
            }

            foreach (var overlay in dummySettings.TextOverlays)
            {
                _settings.TextOverlays.Add(overlay);
            }

            foreach (var overlay in dummySettings.ImageOverlays)
            {
                _settings.ImageOverlays.Add(overlay);
            }
        }

        public void Start(StartCmdOptions startOptions)
        {
            _settings.IncludeCursor = startOptions.Cursor;
            _settings.Clicks.Display = startOptions.Clicks;
            _settings.Keystrokes.Display = startOptions.Keys;

            if (File.Exists(startOptions.FileName))
            {
                if (!startOptions.Overwrite)
                {
                    if (!ServiceProvider.MessageProvider
                        .ShowYesNo("Output File Already Exists, Do you want to overwrite?", ""))
                        return;
                }

                File.Delete(startOptions.FileName);
            }

            HandleVideoSource(startOptions);

            HandleVideoEncoder(startOptions);

            HandleAudioSource(startOptions);

            HandleWebCam(startOptions);

            if (startOptions.FrameRate is int frameRate)
                _settings.Video.FrameRate = frameRate;

            if (startOptions.AudioQuality is int aq)
                _settings.Audio.Quality = aq;

            if (startOptions.VideoQuality is int vq)
                _settings.Video.Quality = vq;

            if (startOptions.Delay > 0)
                Thread.Sleep(startOptions.Delay);

            if (!_recordingModel.StartRecording(startOptions.FileName))
                return;

            Task.Factory.StartNew(() =>
            {
                Loop(startOptions);

                _recordingModel.StopRecording().Wait();

                Application.Exit();
            });

            // MouseKeyHook requires a Window Handle to register
            Application.Run(new ApplicationContext());
        }

        public void Shot(ShotCmdOptions shotOptions)
        {
            _settings.IncludeCursor = shotOptions.Cursor;

            // Screenshot Window with Transparency
            if (shotOptions.Source != null && Regex.IsMatch(shotOptions.Source, @"win:\d+"))
            {
                var ptr = int.Parse(shotOptions.Source.Substring(4));

                try
                {
                    var win = _platformServices.GetWindow(new IntPtr(ptr));
                    var bmp = _screenShotModel.ScreenShotWindow(win);

                    _screenShotModel.SaveScreenShot(bmp, shotOptions.FileName).Wait();
                }
                catch
                {
                    // Suppress Errors
                }
            }
            else
            {
                HandleVideoSource(shotOptions);

                _screenShotModel.CaptureScreenShot(shotOptions.FileName);
            }
        }

        private void HandleVideoSource(CommonCmdOptions commonOptions)
        {
            if (commonOptions.Source == null)
                return;

            var provider = _videoSourceProviders.FirstOrDefault(videoSourceProvider => videoSourceProvider.ParseCli(commonOptions.Source));

            if (provider != null)
            {
                _videoSourcesViewModel.RestoreSourceKind(provider);
            }
        }

        private void HandleAudioSource(StartCmdOptions startOptions)
        {
            var audioSource = ServiceProvider.Get<AudioSource>();

            if (startOptions.Microphone != -1 && startOptions.Microphone < audioSource.AvailableRecordingSources.Count)
            {
                _settings.Audio.Enabled = true;
                audioSource.AvailableRecordingSources[startOptions.Microphone].Active = true;
            }

            if (startOptions.Speaker != -1 && startOptions.Speaker < audioSource.AvailableLoopbackSources.Count)
            {
                _settings.Audio.Enabled = true;
                audioSource.AvailableLoopbackSources[startOptions.Speaker].Active = true;
            }
        }

        private void HandleVideoEncoder(StartCmdOptions startOptions)
        {
            if (startOptions.Encoder == null)
                return;

            // FFmpeg
            if (FFmpegService.FFmpegExists && Regex.IsMatch(startOptions.Encoder, @"^ffmpeg:\d+$"))
            {
                var index = int.Parse(startOptions.Encoder.Substring(7));

                _videoWritersViewModel.SelectedVideoWriterKind = ServiceProvider.Get<FFmpegWriterProvider>();

                if (index < _videoWritersViewModel.AvailableVideoWriters.Count)
                    _videoWritersViewModel.SelectedVideoWriter = _videoWritersViewModel.AvailableVideoWriters[index];
            }

            // SharpAvi
            else if (ServiceProvider.FileExists("SharpAvi.dll") && Regex.IsMatch(startOptions.Encoder, @"^sharpavi:\d+$"))
            {
                var index = int.Parse(startOptions.Encoder.Substring(9));

                _videoWritersViewModel.SelectedVideoWriterKind = ServiceProvider.Get<SharpAviWriterProvider>();

                if (index < _videoWritersViewModel.AvailableVideoWriters.Count)
                    _videoWritersViewModel.SelectedVideoWriter = _videoWritersViewModel.AvailableVideoWriters[index];
            }

            // Gif
            else if (startOptions.Encoder == "gif")
            {
                _videoWritersViewModel.SelectedVideoWriterKind = ServiceProvider.Get<GifWriterProvider>();
            }
        }

        private void HandleWebCam(StartCmdOptions startOptions)
        {
            if (startOptions.WebCam == -1 || startOptions.WebCam >= _webCamProvider.AvailableCams.Count - 1)
            {
                return;
            }

            _webCamProvider.SelectedCam = _webCamProvider.AvailableCams[startOptions.WebCam + 1];

            // Sleep to prevent AccessViolationException
            Thread.Sleep(500);
        }

        private void Loop(StartCmdOptions startOptions)
        {
            if (startOptions.Length > 0)
            {
                var elapsed = 0;

                Write(TimeSpan.Zero);

                while (elapsed++ < startOptions.Length)
                {
                    Thread.Sleep(1000);
                    Write(new string('\b', 8) + TimeSpan.FromSeconds(elapsed));
                }

                Write(new string('\b', 8));
            }
            else
            {
                const string recordingText = "Press p to pause or resume, q to quit";

                WriteLine(recordingText);

                char ReadChar()
                {
                    if (!IsInputRedirected)
                    {
                        return char.ToLower(ReadKey(true).KeyChar);
                    }

                    var line = ReadLine();

                    if (line != null && line.Length == 1)
                    {
                        return line[0];
                    }

                    return char.MinValue;

                }

                char c;

                do
                {
                    c = ReadChar();

                    if (c != 'p')
                        continue;

                    _recordingModel.OnPauseExecute();

                    if (_recordingModel.RecorderState != RecorderState.Paused)
                    {
                        WriteLine("Resumed");
                    }
                } while (c != 'q');
            }
        }
    }
}