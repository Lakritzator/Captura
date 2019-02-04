using FirstFloor.ModernUI.Presentation;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Captura.Base.Services;
using Captura.Core;
using Captura.Core.Settings;
using Captura.Loc;
using Captura.Models;
using Captura.MouseKeyHook;
using Captura.Presentation;
using Captura.ViewCore;
using CommandLine;
using ExceptionWindow = Captura.Windows.ExceptionWindow;

namespace Captura
{
    public partial class App
    {
        public static CmdOptions CmdOptions { get; private set; }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            var dir = Path.Combine(ServiceProvider.SettingsDir, "Crashes");

            Directory.CreateDirectory(dir);

            File.WriteAllText(Path.Combine(dir, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt"), args.Exception.ToString());

            args.Handled = true;

            new ExceptionWindow(args.Exception).ShowDialog();
        }

        private void Application_Startup(object sender, StartupEventArgs args)
        {
            AppDomain.CurrentDomain.UnhandledException += (o, unhandledExceptionEventArgs) =>
            {
                var dir = Path.Combine(ServiceProvider.SettingsDir, "Crashes");

                Directory.CreateDirectory(dir);

                File.WriteAllText(Path.Combine(dir, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt"), unhandledExceptionEventArgs.ExceptionObject.ToString());

                if (unhandledExceptionEventArgs.ExceptionObject is Exception e)
                {
                    Current.Dispatcher.Invoke(() => new ExceptionWindow(e).ShowDialog());
                }

                Shutdown();
            };

            ServiceProvider.LoadModule(new CoreModule());
            ServiceProvider.LoadModule(new ViewCoreModule());

            Parser.Default.ParseArguments<CmdOptions>(args.Args)
                .WithParsed(cmdOptions => CmdOptions = cmdOptions);

            if (CmdOptions.Settings != null)
            {
                ServiceProvider.SettingsDir = CmdOptions.Settings;
            }

            var settings = ServiceProvider.Get<Settings>();

            if (!CmdOptions.Reset)
            {
                settings.Load();
            }

            if (settings.Ui.DarkTheme)
            {
                AppearanceManager.Current.ThemeSource = AppearanceManager.DarkThemeSource;
            }

            var accent = settings.Ui.AccentColor;

            if (!string.IsNullOrEmpty(accent))
            {
                AppearanceManager.Current.AccentColor = WpfExtensions.ParseColor(accent);
            }

            if (!string.IsNullOrWhiteSpace(settings.Ui.Language))
            {
                var matchedCulture = LanguageManager.Instance.AvailableCultures.FirstOrDefault(cultureInfo => cultureInfo.Name == settings.Ui.Language);

                if (matchedCulture != null)
                    LanguageManager.Instance.CurrentCulture = matchedCulture;
            }

            LanguageManager.Instance.LanguageChanged += cultureInfo => settings.Ui.Language = cultureInfo.Name;

            var keymap = ServiceProvider.Get<KeymapViewModel>();

            if (!string.IsNullOrWhiteSpace(settings.Keystrokes.KeymapName))
            {
                var matched = keymap.AvailableKeymaps.FirstOrDefault(keymapItem => keymapItem.Name == settings.Keystrokes.KeymapName);

                if (matched != null)
                    keymap.SelectedKeymap = matched;
            }

            keymap.PropertyChanged += (o, e) => settings.Keystrokes.KeymapName = keymap.SelectedKeymap.Name;
        }
    }
}