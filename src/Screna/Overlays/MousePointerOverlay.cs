using System;
using System.Drawing;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Settings;

namespace Screna.Overlays
{
    public class MousePointerOverlay : IOverlay
    {
        private readonly MouseOverlaySettings _settings;
        
        public MousePointerOverlay(MouseOverlaySettings settings)
        {
            _settings = settings;
        }
        
        /// <summary>
        /// Draws overlay.
        /// </summary>
        public void Draw(IEditableFrame editor, Func<Point, Point> transform = null)
        {
            if (!_settings.Display)
                return;

            var clickRadius = _settings.Radius;

            var curPos = MouseCursor.CursorPosition;

            if (transform != null)
                curPos = transform(curPos);

            var d = clickRadius * 2;

            var x = curPos.X - clickRadius;
            var y = curPos.Y - clickRadius;

            editor.FillEllipse(_settings.Color, new RectangleF(x, y, d, d));

            var border = _settings.BorderThickness;

            if (border <= 0)
            {
                return;
            }

            x -= border / 2;
            y -= border / 2;
            d += border;

            editor.DrawEllipse(_settings.BorderColor, border, new RectangleF(x, y, d, d));
        }

        public void Dispose() { }
    }
}
