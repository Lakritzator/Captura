using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using Captura.Base;
using Captura.Core.Settings;
using Captura.Core.ViewModels;
using Captura.Loc;

namespace Captura.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AboutViewModel : ViewModelBase
    {
        public ICommand HyperlinkCommand { get; }

        public static Version Version { get; }

        public string AppVersion { get; }

        static AboutViewModel()
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version;
        }

        public AboutViewModel(Settings settings, LanguageManager languageManager) : base(settings, languageManager)
        {
            AppVersion = "v" + Version.ToString(3);

            HyperlinkCommand = new DelegateCommand(link =>
            {
                if (link is string s)
                {
                    try
                    {
                        Process.Start(s);
                    }
                    catch
                    {
                        // Suppress Errors
                    }
                }
            });
        }
    }
}
