using System.Text;
using Captura.Base;
using Captura.Loc;

namespace Captura.HotKeys
{
    public class Service : NotifyPropertyChanged
    {
        private readonly LanguageManager _loc;

        public Service(ServiceName serviceName)
        {
            ServiceName = serviceName;
            _loc = LanguageManager.Instance;

            _loc.LanguageChanged += cultureInfo => RaisePropertyChanged(nameof(Description));
        }

        private ServiceName _serviceName;

        public ServiceName ServiceName
        {
            get => _serviceName;
            set
            {
                _serviceName = value;

                OnPropertyChanged();

                RaisePropertyChanged(nameof(Description));
            }
        }

        public string Description => GetDescription();

        private string GetDescription()
        {
            switch (ServiceName)
            {
                case ServiceName.None:
                    return _loc.None;

                case ServiceName.Recording:
                    return _loc.StartStopRecording;

                case ServiceName.Pause:
                    return _loc.PauseResumeRecording;

                case ServiceName.ScreenShot:
                    return _loc.ScreenShot;

                case ServiceName.ActiveScreenShot:
                    return _loc.ScreenShotActiveWindow;

                case ServiceName.DesktopScreenShot:
                    return _loc.ScreenShotDesktop;

                case ServiceName.ToggleMouseClicks:
                    return _loc.ToggleMouseClicks;

                case ServiceName.ToggleKeystrokes:
                    return _loc.ToggleKeystrokes;

                case ServiceName.ScreenShotRegion:
                    return "ScreenShot (Region)";

                case ServiceName.ScreenShotScreen:
                    return "ScreenShot (Screen)";

                case ServiceName.ScreenShotWindow:
                    return "ScreenShot (Window)";

                default:
                    return SpaceAtCapitals(ServiceName);
            }
        }

        private static string SpaceAtCapitals<T>(T obj)
        {
            var s = obj.ToString();

            var sb = new StringBuilder();

            for (var i = 0; i < s.Length; ++i)
            {
                if (i != 0 && char.IsUpper(s[i]))
                    sb.Append(" ");

                sb.Append(s[i]);
            }

            return sb.ToString();
        }
    }
}