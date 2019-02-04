using System;
using System.Collections.Generic;
using System.Drawing;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Settings;
using Screna.Overlays.Settings;

namespace Screna.Overlays
{
    public class CensorOverlay : IOverlay
    {
        private readonly IEnumerable<CensorOverlaySettings> _overlaySettings;

        public CensorOverlay(IEnumerable<CensorOverlaySettings> overlaySettings)
        {
            _overlaySettings = overlaySettings;
        }

        public void Dispose() { }

        private static float GetLeft(CensorOverlaySettings overlaySettings, float fullWidth)
        {
            var x = overlaySettings.X;

            switch (overlaySettings.HorizontalAlignment)
            {
                case Alignment.Start:
                    return x;

                case Alignment.End:
                    return fullWidth - x - overlaySettings.Width;

                case Alignment.Center:
                    return fullWidth / 2 + x - overlaySettings.Width / 2f;

                default:
                    return 0;
            }
        }

        private static float GetTop(CensorOverlaySettings overlaySettings, float fullHeight)
        {
            var y = overlaySettings.Y;

            switch (overlaySettings.VerticalAlignment)
            {
                case Alignment.Start:
                    return y;

                case Alignment.End:
                    return fullHeight - y - overlaySettings.Height;

                case Alignment.Center:
                    return fullHeight / 2 + y - overlaySettings.Height / 2f;

                default:
                    return 0;
            }
        }

        public void Draw(IEditableFrame editor, Func<Point, Point> pointTransform = null)
        {
            foreach (var overlaySetting in _overlaySettings)
            {
                if (!overlaySetting.Display)
                    continue;

                editor.FillRectangle(Color.Black,
                    new RectangleF(
                        GetLeft(overlaySetting, editor.Width),
                        GetTop(overlaySetting, editor.Height),
                        overlaySetting.Width,
                        overlaySetting.Height));
            }
        }
    }
}