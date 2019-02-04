using Screna.Overlays.Settings;

namespace Captura.Core.Settings.Models
{
    public class WebCamOverlaySettings : ImageOverlaySettings
    {
        public bool SeparateFile
        {
            get => Get(false);
            set => Set(value);
        }
    }
}