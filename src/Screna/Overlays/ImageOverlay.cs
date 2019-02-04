using System;
using System.Drawing;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Settings;
using Screna.Overlays.Settings;

namespace Screna.Overlays
{
    public abstract class ImageOverlay<T> : IOverlay where T : ImageOverlaySettings
    {
        protected readonly T Settings;
        private readonly bool _disposeImages;

        protected ImageOverlay(T settings, bool disposeImages)
        {
            _disposeImages = disposeImages;

            Settings = settings;
        }

        public void Draw(IEditableFrame editor, Func<Point, Point> pointTransform = null)
        {
            var img = GetImage(editor, out var targetSize);

            if (img == null)
                return;

            try
            {
                if (Settings.Resize)
                    targetSize = new Size(Settings.ResizeWidth, Settings.ResizeHeight);

                var point = GetPosition(new Size((int)editor.Width, (int)editor.Height), targetSize);
                var destRect = new Rectangle(point, targetSize);

                editor.DrawImage(img, destRect, Settings.Opacity);
            }
            catch { }
            finally
            {
                if (_disposeImages)
                    img.Dispose();
            }
        }

        protected abstract IDisposable GetImage(IEditableFrame editor, out Size size);

        private Point GetPosition(Size bounds, Size imageSize)
        {
            var point = new Point(Settings.X, Settings.Y);

            switch (Settings.HorizontalAlignment)
            {
                case Alignment.Center:
                    point.X = bounds.Width / 2 - imageSize.Width / 2 + point.X;
                    break;

                case Alignment.End:
                    point.X = bounds.Width - imageSize.Width - point.X;
                    break;
            }

            switch (Settings.VerticalAlignment)
            {
                case Alignment.Center:
                    point.Y = bounds.Height / 2 - imageSize.Height / 2 + point.Y;
                    break;

                case Alignment.End:
                    point.Y = bounds.Height - imageSize.Height - point.Y;
                    break;
            }

            return point;
        }

        public virtual void Dispose() { }
    }
}