using System;
using System.Drawing;
using Captura.Base;
using Captura.Base.Images;
using Screna.Frames;
using Screna.ImageProviders;

namespace Screna
{
    /// <summary>
    /// Contains methods for taking ScreenShots
    /// </summary>
    public static class ScreenShot
    {
        /// <summary>
        /// Captures the entire Desktop.
        /// </summary>
        /// <param name="includeCursor">Whether to include the Mouse Cursor.</param>
        /// <returns>The Captured Image.</returns>
        public static IBitmapImage Capture(bool includeCursor = false)
        {
            return Capture(WindowProvider.DesktopRectangle, includeCursor);
        }

        /// <summary>
        /// Capture transparent Screenshot of a Window.
        /// </summary>
        /// <param name="window">The <see cref="IWindow"/> to Capture.</param>
        /// <param name="includeCursor">Whether to include Mouse Cursor.</param>
        public static IBitmapImage CaptureTransparent(IWindow window, bool includeCursor = false)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            var backdrop = new WindowScreenShotBackdrop(window);

            backdrop.ShowWhite();

            var r = backdrop.Rectangle;

            // Capture screenshot with white background
            using (var whiteShot = CaptureInternal(r))
            {
                backdrop.ShowBlack();

                // Capture screenshot with black background
                using (var blackShot = CaptureInternal(r))
                {
                    backdrop.Dispose();

                    var transparentImage = Extensions.DifferentiateAlpha(whiteShot, blackShot);

                    if (transparentImage == null)
                        return null;

                    // Include Cursor only if within window
                    if (includeCursor && r.Contains(MouseCursor.CursorPosition))
                    {
                        using (var graphics = Graphics.FromImage(transparentImage))
                            MouseCursor.Draw(graphics, point => new Point(point.X - r.X, point.Y - r.Y));
                    }

                    return new DrawingImage(transparentImage.CropEmptyEdges());
                }
            }
        }

        private static Bitmap CaptureInternal(Rectangle region, bool includeCursor = false)
        {
            var bitmap = new Bitmap(region.Width, region.Height);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(region.Location, Point.Empty, region.Size, CopyPixelOperation.SourceCopy);

                if (includeCursor)
                {
                    MouseCursor.Draw(graphics, point => new Point(point.X - region.X, point.Y - region.Y));
                }

                graphics.Flush();
            }

            return bitmap;
        }

        /// <summary>
        /// Captures a Specific Region.
        /// </summary>
        /// <param name="region">A <see cref="Rectangle"/> specifying the Region to Capture.</param>
        /// <param name="includeCursor">Whether to include the Mouse Cursor.</param>
        /// <returns>The Captured Image.</returns>
        public static IBitmapImage Capture(Rectangle region, bool includeCursor = false)
        {
            return new DrawingImage(CaptureInternal(region, includeCursor));
        }
    }
}
