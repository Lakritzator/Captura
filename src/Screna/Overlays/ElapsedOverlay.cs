using System;
using Captura.Base.Settings;

namespace Screna.Overlays
{
    public class ElapsedOverlay : TextOverlay
    {
        private readonly Func<TimeSpan> _elapsed;

        public ElapsedOverlay(TextOverlaySettings overlaySettings, Func<TimeSpan> elapsed) : base(overlaySettings)
        {
            _elapsed = elapsed;
        }

        protected override string GetText() => _elapsed().ToString();
    }
}