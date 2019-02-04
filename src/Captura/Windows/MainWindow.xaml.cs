using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Captura.Base.Services;
using Captura.Core.Settings;
using Captura.Core.ViewModels;
using Captura.FFmpeg;
using Captura.HotKeys;
using Captura.ImageEditor;
using Captura.Models;
using Captura.Presentation;
using Captura.ViewCore;
using Captura.ViewCore.ViewModels;

namespace Captura.Windows
{
    public partial class MainWindow
    {
        public static MainWindow Instance { get; private set; }

        private FFmpegDownloaderWindow _downloader;

        public MainWindow()
        {
            Instance = this;
            
            FFmpegService.FFmpegDownloader += () =>
            {
                if (_downloader == null)
                {
                    _downloader = new FFmpegDownloaderWindow();
                    _downloader.Closed += (sender, args) => _downloader = null;
                }

                _downloader.ShowAndFocus();
            };
            
            InitializeComponent();

            if (DataContext is MainViewModel)
            {
                var mainModel = ServiceProvider.Get<MainModel>();

                mainModel.Init(!App.CmdOptions.NoPersist, !App.CmdOptions.Reset, !App.CmdOptions.NoHotKeys);
                ServiceProvider.Get<HotKeyActionRegisterer>().Register();
                ServiceProvider.Get<TimerModel>().Init();

                var listener = new HotKeyListener();

                var hotKeyManager = ServiceProvider.Get<HotKeyManager>();

                listener.HotKeyReceived += id => hotKeyManager.ProcessHotKey(id);

                ServiceProvider.Get<HotKeyManager>().HotKeyPressed += service =>
                {
                    switch (service)
                    {
                        case ServiceName.OpenImageEditor:
                            new ImageEditorWindow().ShowAndFocus();
                            break;

                        case ServiceName.ShowMainWindow:
                            this.ShowAndFocus();
                            break;
                    }
                };

                Loaded += (sender, args) =>
                {
                    RepositionWindowIfOutside();

                    mainModel.ViewLoaded();
                };
            }

            if (App.CmdOptions.Tray || ServiceProvider.Get<Settings>().Tray.MinToTrayOnStartup)
                Hide();

            Closing += (sender, args) =>
            {
                if (!TryExit())
                    args.Cancel = true;
            };
        }

        private void RepositionWindowIfOutside()
        {
            // Window dimensions taking care of DPI
            var rect = new Rectangle((int)(Left * Dpi.X),
                (int)(Top * Dpi.Y),
                (int)(ActualWidth * Dpi.X),
                (int)(ActualHeight * Dpi.Y));
            
            if (!Screen.AllScreens.Any(screen => screen.Bounds.Contains(rect)))
            {
                Left = 50;
                Top = 50;
            }
        }

        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            DragMove();

            args.Handled = true;
        }

        private void MinButton_Click(object sender, RoutedEventArgs args) => SystemCommands.MinimizeWindow(this);

        private void CloseButton_Click(object sender, RoutedEventArgs args)
        {
            if (ServiceProvider.Get<Settings>().Tray.MinToTrayOnClose)
            {
                Hide();
            }
            else Close();
        }

        private void SystemTray_TrayMouseDoubleClick(object sender, RoutedEventArgs args)
        {
            if (Visibility == Visibility.Visible)
            {
                Hide();
            }
            else
            {
                Show();

                WindowState = WindowState.Normal;

                Activate();
            }
        }

        private bool TryExit()
        {
            var recordingModel = ServiceProvider.Get<RecordingModel>();

            if (!recordingModel.CanExit())
                return false;

            ServiceProvider.Dispose();

            SystemTray.Dispose();

            return true;
        }

        private void MenuExit_Click(object sender, RoutedEventArgs args) => Close();

        private void HideButton_Click(object sender, RoutedEventArgs args) => Hide();

        private void ShowMainWindow(object sender, RoutedEventArgs e)
        {
            this.ShowAndFocus();
        }
    }
}
