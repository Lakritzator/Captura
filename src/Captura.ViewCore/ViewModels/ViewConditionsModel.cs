using System.Reactive.Linq;
using Captura.Base.Audio;
using Captura.Core.Models;
using Captura.Core.Models.Discard;
using Captura.Core.Settings;
using Captura.Core.ViewModels;
using Captura.FFmpeg.Video;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Screna.Gif;
using Screna.VideoSourceProviders;

namespace Captura.ViewCore.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ViewConditionsModel
    {
        public ViewConditionsModel(VideoSourcesViewModel videoSourcesViewModel,
            VideoWritersViewModel videoWritersViewModel,
            Settings settings,
            RecordingModel recordingModel,
            AudioSource audioSource)
        {
            IsRegionMode = videoSourcesViewModel
                .ObserveProperty(sourcesViewModel => sourcesViewModel.SelectedVideoSourceKind)
                .Select(videoSourceProvider => videoSourceProvider is RegionSourceProvider)
                .ToReadOnlyReactivePropertySlim();

            IsAudioMode = videoSourcesViewModel
                .ObserveProperty(sourcesViewModel => sourcesViewModel.SelectedVideoSourceKind)
                .Select(videoSourceProvider => videoSourceProvider is NoVideoSourceProvider)
                .ToReadOnlyReactivePropertySlim();

            MultipleVideoWriters = videoWritersViewModel.AvailableVideoWriters
                .ObserveProperty(readOnlyObservableCollection => readOnlyObservableCollection.Count)
                .Select(i => i > 1)
                .ToReadOnlyReactivePropertySlim();

            IsFFmpeg = videoWritersViewModel
                .ObserveProperty(writersViewModel => writersViewModel.SelectedVideoWriterKind)
                .Select(videoWriterProvider => videoWriterProvider is FFmpegWriterProvider || videoWriterProvider is StreamingWriterProvider)
                .ToReadOnlyReactivePropertySlim();

            IsGifMode = videoWritersViewModel
                .ObserveProperty(writersViewModel => writersViewModel.SelectedVideoWriterKind)
                .Select(videoWriterProvider => videoWriterProvider is GifWriterProvider)
                .ToReadOnlyReactivePropertySlim();

            CanSelectFrameRate = new[]
                {
                    videoWritersViewModel
                        .ObserveProperty(writersViewModel => writersViewModel.SelectedVideoWriterKind)
                        .Select(videoWriterProvider => videoWriterProvider is GifWriterProvider),
                    settings.Gif
                        .ObserveProperty(gifSettings => gifSettings.VariableFrameRate)
                }
                .CombineLatestValuesAreAllTrue()
                .Select(b => !b)
                .ToReadOnlyReactivePropertySlim();

            IsVideoQuality = videoWritersViewModel
                .ObserveProperty(writersViewModel => writersViewModel.SelectedVideoWriterKind)
                .Select(videoWriterProvider => !(videoWriterProvider is GifWriterProvider || videoWriterProvider is DiscardWriterProvider))
                .ToReadOnlyReactivePropertySlim();

            CanChangeWebCam = new[]
                {
                    recordingModel
                        .ObserveProperty(model => model.RecorderState)
                        .Select(recorderState => recorderState == RecorderState.NotRecording),
                    settings.WebCamOverlay
                        .ObserveProperty(webCamOverlaySettings => webCamOverlaySettings.SeparateFile)
                }
                .CombineLatest(bools => !bools[1] || bools[0]) // Not SeparateFile or NotRecording
                .ToReadOnlyReactivePropertySlim();

            CanChangeAudioSources = new[]
                {
                    recordingModel
                        .ObserveProperty(model => model.RecorderState)
                        .Select(recorderState => recorderState == RecorderState.NotRecording),
                    settings.Audio
                        .ObserveProperty(audioSettings => audioSettings.SeparateFilePerSource)
                }
                .CombineLatest(bools =>
                    audioSource.CanChangeSourcesDuringRecording ||
                    !bools[1] || bools[0]) // Not SeparateFilePerSource or NotRecording
                .ToReadOnlyReactivePropertySlim();

            IsEnabled = recordingModel
                .ObserveProperty(model => model.RecorderState)
                .Select(recorderState => recorderState == RecorderState.NotRecording)
                .ToReadOnlyReactivePropertySlim();
        }

        public IReadOnlyReactiveProperty<bool> IsRegionMode { get; }

        public IReadOnlyReactiveProperty<bool> IsAudioMode { get; }

        public IReadOnlyReactiveProperty<bool> MultipleVideoWriters { get; }

        public IReadOnlyReactiveProperty<bool> IsGifMode { get; }

        public IReadOnlyReactiveProperty<bool> IsFFmpeg { get; }

        public IReadOnlyReactiveProperty<bool> CanSelectFrameRate { get; }

        public IReadOnlyReactiveProperty<bool> IsVideoQuality { get; }

        public IReadOnlyReactiveProperty<bool> CanChangeWebCam { get; }

        public IReadOnlyReactiveProperty<bool> CanChangeAudioSources { get; }

        public IReadOnlyReactiveProperty<bool> IsEnabled { get; }
    }
}