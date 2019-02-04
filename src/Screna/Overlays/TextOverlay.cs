using System;
using System.Drawing;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Settings;

namespace Screna.Overlays
{
    public abstract class TextOverlay : IOverlay
    {
        private readonly TextOverlaySettings _overlaySettings;

        protected TextOverlay(TextOverlaySettings overlaySettings)
        {
            _overlaySettings = overlaySettings;
        }

        public virtual void Dispose() { }

        private static float GetLeft(TextOverlaySettings overlaySettings, float fullWidth, float textWidth)
        {
            var x = overlaySettings.X;

            switch (overlaySettings.HorizontalAlignment)
            {
                case Alignment.Start:
                    return x;

                case Alignment.End:
                    return fullWidth - x - textWidth - 2 * overlaySettings.HorizontalPadding;

                case Alignment.Center:
                    return fullWidth / 2 + x - textWidth / 2 - overlaySettings.HorizontalPadding;

                default:
                    return 0;
            }
        }

        private static float GetTop(TextOverlaySettings overlaySettings, float fullHeight, float textHeight)
        {
            var y = overlaySettings.Y;

            switch (overlaySettings.VerticalAlignment)
            {
                case Alignment.Start:
                    return y;

                case Alignment.End:
                    return fullHeight - y - textHeight - 2 * overlaySettings.VerticalPadding;

                case Alignment.Center:
                    return fullHeight / 2 + y - textHeight / 2 - overlaySettings.VerticalPadding;

                default:
                    return 0;
            }
        }

        protected abstract string GetText();
        
        public virtual void Draw(IEditableFrame editor, Func<Point, Point> pointTransform = null)
        {
            if (!_overlaySettings.Display)
                return;

            var text = GetText();

            if (string.IsNullOrWhiteSpace(text))
                return;

            var fontSize = _overlaySettings.FontSize;

            var size = editor.MeasureString(text, fontSize);

            int paddingX = _overlaySettings.HorizontalPadding, paddingY = _overlaySettings.VerticalPadding;

            var rect = new RectangleF(GetLeft(_overlaySettings, editor.Width, size.Width),
                GetTop(_overlaySettings, editor.Height, size.Height),
                size.Width + 2 * paddingX,
                size.Height + 2 * paddingY);

            editor.FillRectangle(_overlaySettings.BackgroundColor,
                rect,
                _overlaySettings.CornerRadius);

            editor.DrawString(text,
                fontSize,
                _overlaySettings.FontColor,
                new RectangleF(rect.Left + paddingX, rect.Top + paddingY, size.Width, size.Height));

            var border = _overlaySettings.BorderThickness;

            if (border <= 0)
            {
                return;
            }

            rect = new RectangleF(rect.Left - border / 2f, rect.Top - border / 2f, rect.Width + border, rect.Height + border);

            editor.DrawRectangle(_overlaySettings.BorderColor, border,
                rect,
                _overlaySettings.CornerRadius);
        }
    }
}