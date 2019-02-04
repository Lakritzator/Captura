using Captura.Base.Settings;

namespace Screna.Overlays.Settings
{
    public class CustomOverlaySettings : TextOverlaySettings
    {
        public string Text
        {
            get => Get("");
            set => Set(value);
        }
    }
}