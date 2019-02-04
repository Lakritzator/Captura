using Screna.Overlays.Settings;

namespace Screna.Overlays
{
    public class CustomOverlay : TextOverlay
    {
        private readonly CustomOverlaySettings _overlaySettings;
        
        public CustomOverlay(CustomOverlaySettings overlaySettings) : base(overlaySettings)
        {
            _overlaySettings = overlaySettings;
        }

        protected override string GetText() => _overlaySettings.Text;
    }
}