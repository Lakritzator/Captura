using Captura.Core;
using Captura.Core.Settings;
using Captura.HotKeys;
using Captura.ViewCore.ViewModels;

namespace Captura.ViewCore
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class HotKeyActionRegisterer
    {
        private readonly ScreenShotViewModel _screenShotViewModel;
        private readonly HotKeyManager _hotKeyManager;
        private readonly RecordingViewModel _recordingViewModel;
        private readonly Settings _settings;

        public HotKeyActionRegisterer(ScreenShotViewModel screenShotViewModel,
            HotKeyManager hotKeyManager,
            RecordingViewModel recordingViewModel,
            Settings settings)
        {
            _screenShotViewModel = screenShotViewModel;
            _hotKeyManager = hotKeyManager;
            _recordingViewModel = recordingViewModel;
            _settings = settings;
        }

        public void Register()
        {
            _hotKeyManager.HotKeyPressed += service =>
            {
                switch (service)
                {
                    case ServiceName.Recording:
                        _recordingViewModel.RecordCommand.ExecuteIfCan();
                        break;

                    case ServiceName.Pause:
                        _recordingViewModel.PauseCommand.ExecuteIfCan();
                        break;

                    case ServiceName.ScreenShot:
                        _screenShotViewModel.ScreenShotCommand.ExecuteIfCan();
                        break;

                    case ServiceName.ActiveScreenShot:
                        _screenShotViewModel.ScreenShotActiveCommand.ExecuteIfCan();
                        break;

                    case ServiceName.DesktopScreenShot:
                        _screenShotViewModel.ScreenShotDesktopCommand.ExecuteIfCan();
                        break;

                    case ServiceName.ToggleMouseClicks:
                        _settings.Clicks.Display = !_settings.Clicks.Display;
                        break;

                    case ServiceName.ToggleKeystrokes:
                        _settings.Keystrokes.Display = !_settings.Keystrokes.Display;
                        break;

                    case ServiceName.ScreenShotRegion:
                        _screenShotViewModel.ScreenshotRegionCommand.ExecuteIfCan();
                        break;

                    case ServiceName.ScreenShotScreen:
                        _screenShotViewModel.ScreenshotScreenCommand.ExecuteIfCan();
                        break;

                    case ServiceName.ScreenShotWindow:
                        _screenShotViewModel.ScreenshotWindowCommand.ExecuteIfCan();
                        break;
                }
            };
        }
    }
}