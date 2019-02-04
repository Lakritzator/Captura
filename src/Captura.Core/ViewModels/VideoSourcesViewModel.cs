using System.Collections.Generic;
using System.Threading;
using Captura.Base;
using Captura.Base.Video;
using Captura.Core.Models;
using Screna.VideoSourceProviders;

namespace Captura.Core.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class VideoSourcesViewModel : NotifyPropertyChanged
    {
        readonly FullScreenSourceProvider _fullScreenProvider;
        public NoVideoSourceProvider NoVideoSourceProvider { get; }

        readonly SynchronizationContext _syncContext = SynchronizationContext.Current;

        public IEnumerable<IVideoSourceProvider> VideoSources { get; }

        public VideoSourcesViewModel(FullScreenSourceProvider fullScreenProvider,
            NoVideoSourceProvider noVideoSourceProvider,
            IEnumerable<IVideoSourceProvider> sourceProviders)
        {
            NoVideoSourceProvider = noVideoSourceProvider;
            _fullScreenProvider = fullScreenProvider;
            VideoSources = sourceProviders;

            SetDefaultSource();
        }

        public void SetDefaultSource()
        {
            SelectedVideoSourceKind = _fullScreenProvider;
        }

        void ChangeSource(IVideoSourceProvider newSourceProvider, bool callOnSelect)
        {
            try
            {
                if (newSourceProvider == null || _videoSourceKind == newSourceProvider)
                    return;

                if (callOnSelect && !newSourceProvider.OnSelect())
                {
                    return;
                }

                if (_videoSourceKind != null)
                {
                    _videoSourceKind.OnUnselect();

                    _videoSourceKind.UnselectRequested -= SetDefaultSource;
                }

                _videoSourceKind = newSourceProvider;

                _videoSourceKind.UnselectRequested += SetDefaultSource;
            }
            finally
            {
                // Important to send PropertyChanged event over SynchronizationContext for consistency in UI

                void PropChange()
                {
                    RaisePropertyChanged(nameof(SelectedVideoSourceKind));
                }

                if (_syncContext != null)
                {
                    _syncContext.Post(state => PropChange(), null);
                }
                else PropChange();
            }
        }

        IVideoSourceProvider _videoSourceKind;

        public IVideoSourceProvider SelectedVideoSourceKind
        {
            get => _videoSourceKind;
            set => ChangeSource(value, true);
        }

        public void RestoreSourceKind(IVideoSourceProvider sourceProvider)
        {
            ChangeSource(sourceProvider, false);
        }
    }
}