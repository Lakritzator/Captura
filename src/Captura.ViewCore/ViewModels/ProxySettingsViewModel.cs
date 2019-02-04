using System.Collections.Generic;
using Captura.Base.Settings;
using Captura.Core.Settings;
using Captura.Core.ViewModels;
using Captura.Loc;

namespace Captura.ViewCore.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ProxySettingsViewModel : ViewModelBase
    {
        public ProxySettingsViewModel(Settings settings, LanguageManager languageManager)
            : base(settings, languageManager)
        {
            settings.Proxy.PropertyChanged += (sender, e) => RaiseAllChanged();
        }

        public ProxySettings ProxySettings => Settings.Proxy;

        public bool CanAuth => ProxySettings.Type != ProxyType.None;

        public bool CanAuthCred => CanAuth && ProxySettings.Authenticate;

        public bool CanHost => ProxySettings.Type == ProxyType.Manual;

        public IEnumerable<ProxyType> ProxyTypes { get; } = new[] { ProxyType.None, ProxyType.System, ProxyType.Manual };
    }
}