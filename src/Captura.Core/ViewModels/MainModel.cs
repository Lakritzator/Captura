using System;
using System.IO;
using System.Linq;
using Captura.Base;
using Captura.Base.Audio;
using Captura.Base.Services;
using Captura.Core.Models;
using Captura.HotKeys;

namespace Captura.Core.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MainModel : NotifyPropertyChanged, IDisposable
    {
        readonly Settings.Settings _settings;
        bool _persist, _hotKeys, _remembered;

        readonly RememberByName _rememberByName;

        readonly IWebCamProvider _webCamProvider;
        readonly VideoWritersViewModel _videoWritersViewModel;
        readonly AudioSource _audioSource;
        readonly HotKeyManager _hotKeyManager;

        public MainModel(Settings.Settings settings,
            IWebCamProvider webCamProvider,
            VideoWritersViewModel videoWritersViewModel,
            AudioSource audioSource,
            HotKeyManager hotKeyManager,
            RememberByName rememberByName)
        {
            _settings = settings;
            _webCamProvider = webCamProvider;
            _videoWritersViewModel = videoWritersViewModel;
            _audioSource = audioSource;
            _hotKeyManager = hotKeyManager;
            _rememberByName = rememberByName;

            // If Output Directory is not set. Set it to Documents\Captura\
            if (string.IsNullOrWhiteSpace(settings.OutPath))
                settings.OutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Captura");

            // Create the Output Directory if it does not exist
            settings.EnsureOutPath();
        }

        public void Refresh()
        {
            _videoWritersViewModel.RefreshCodecs();

            _audioSource.Refresh();

            #region WebCam
            var lastWebCamName = _webCamProvider.SelectedCam?.Name;

            _webCamProvider.Refresh();

            var matchingWebCam = _webCamProvider.AvailableCams.FirstOrDefault(webCamItem => webCamItem.Name == lastWebCamName);

            if (matchingWebCam != null)
            {
                _webCamProvider.SelectedCam = matchingWebCam;
            }
            #endregion
        }

        public void Init(bool persist, bool remembered, bool hotKeys)
        {
            _persist = persist;
            _hotKeys = hotKeys;

            // Register HotKeys if not console
            if (_hotKeys)
                _hotKeyManager.RegisterAll();

            if (remembered)
            {
                _remembered = true;

                _rememberByName.RestoreRemembered();
            }
        }

        public void ViewLoaded()
        {
            if (_remembered)
            {
                // Restore WebCam
                if (!string.IsNullOrEmpty(_settings.Video.WebCam))
                {
                    var webCam = _webCamProvider.AvailableCams.FirstOrDefault(webCamItem => webCamItem.Name == _settings.Video.WebCam);

                    if (webCam != null)
                    {
                        _webCamProvider.SelectedCam = webCam;
                    }
                }
            }

            _hotKeyManager.ShowNotRegisteredOnStartup();
        }

        public void Dispose()
        {
            // Remember things if not console.
            if (_persist)
            {
                _rememberByName.Remember();

                _settings.Save();
            }
        }
    }
}