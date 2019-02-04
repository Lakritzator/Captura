using System.Collections.Generic;
using System.Linq;
using Captura.Base.Audio;
using Captura.Base.Services;
using Captura.Base.Video;
using Captura.Core.ViewModels;

namespace Captura.Core.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RememberByName
    {
        readonly Settings.Settings _settings;
        readonly VideoSourcesViewModel _videoSourcesViewModel;
        readonly VideoWritersViewModel _videoWritersViewModel;
        readonly AudioSource _audioSource;
        readonly IWebCamProvider _webCamProvider;
        readonly ScreenShotModel _screenShotModel;
        readonly IEnumerable<IVideoSourceProvider> _videoSourceProviders;

        public RememberByName(Settings.Settings settings,
            VideoSourcesViewModel videoSourcesViewModel,
            VideoWritersViewModel videoWritersViewModel,
            AudioSource audioSource,
            IWebCamProvider webCamProvider,
            ScreenShotModel screenShotModel,
            IEnumerable<IVideoSourceProvider> videoSourceProviders)
        {
            _settings = settings;
            _videoSourcesViewModel = videoSourcesViewModel;
            _videoWritersViewModel = videoWritersViewModel;
            _audioSource = audioSource;
            _webCamProvider = webCamProvider;
            _screenShotModel = screenShotModel;
            _videoSourceProviders = videoSourceProviders;
        }

        public void Remember()
        {
            // Remember Video Source
            _settings.Video.SourceKind = _videoSourcesViewModel.SelectedVideoSourceKind.Name;
            _settings.Video.Source = _videoSourcesViewModel.SelectedVideoSourceKind.Serialize();

            // Remember Video Codec
            _settings.Video.WriterKind = _videoWritersViewModel.SelectedVideoWriterKind.Name;
            _settings.Video.Writer = _videoWritersViewModel.SelectedVideoWriter.ToString();

            // Remember Audio Sources
            _settings.Audio.Microphones = _audioSource.AvailableRecordingSources
                .Where(audioItem => audioItem.Active)
                .Select(audioItem => audioItem.Name)
                .ToArray();

            _settings.Audio.Speakers = _audioSource.AvailableLoopbackSources
                .Where(audioItem => audioItem.Active)
                .Select(audioItem => audioItem.Name)
                .ToArray();

            // Remember ScreenShot Target
            _settings.ScreenShots.SaveTargets = _screenShotModel.AvailableImageWriters
                .Where(imageWriterItem => imageWriterItem.Active)
                .Select(imageWriterItem => imageWriterItem.Display)
                .ToArray();

            // Remember WebCam
            _settings.Video.WebCam = _webCamProvider.SelectedCam.Name;
        }

        void RestoreVideoSource()
        {
            if (string.IsNullOrEmpty(_settings.Video.SourceKind))
                return;

            var provider = _videoSourceProviders.FirstOrDefault(videoSourceProvider => videoSourceProvider.Name == _settings.Video.SourceKind);

            if (provider == null)
                return;

            if (provider.Deserialize(_settings.Video.Source))
            {
                _videoSourcesViewModel.RestoreSourceKind(provider);
            }
        }

        void RestoreVideoCodec()
        {
            if (string.IsNullOrEmpty(_settings.Video.WriterKind))
                return;

            var kind = _videoWritersViewModel.VideoWriterProviders.FirstOrDefault(videoWriterProvider => videoWriterProvider.Name == _settings.Video.WriterKind);

            if (kind == null)
                return;

            _videoWritersViewModel.SelectedVideoWriterKind = kind;

            var codec = _videoWritersViewModel.AvailableVideoWriters.FirstOrDefault(videoWriterItem => videoWriterItem.ToString() == _settings.Video.Writer);

            if (codec != null)
                _videoWritersViewModel.SelectedVideoWriter = codec;
        }

        public void RestoreRemembered()
        {
            RestoreVideoSource();

            RestoreVideoCodec();

            // Restore Microphones
            if (_settings.Audio.Microphones != null)
            {
                foreach (var source in _audioSource.AvailableRecordingSources)
                {
                    source.Active = _settings.Audio.Microphones.Contains(source.Name);
                }
            }

            // Restore Loopback Speakers
            if (_settings.Audio.Speakers != null)
            {
                foreach (var source in _audioSource.AvailableLoopbackSources)
                {
                    source.Active = _settings.Audio.Speakers.Contains(source.Name);
                }
            }

            // Restore ScreenShot Target
            if (_settings.ScreenShots.SaveTargets != null)
            {
                foreach (var imageWriter in _screenShotModel.AvailableImageWriters)
                {
                    imageWriter.Active = _settings.ScreenShots.SaveTargets.Contains(imageWriter.Display);
                }

                // Activate First if none
                if (!_screenShotModel.AvailableImageWriters.Any(imageWriterItem => imageWriterItem.Active))
                {
                    _screenShotModel.AvailableImageWriters[0].Active = true;
                }
            }
        }
    }
}