using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Captura.Base.Images;

namespace Screna
{
    public static class GraphicsExtensions
    {
        private static GraphicsPath RoundedRect(RectangleF bounds, int radius)
        {
            var path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            var diameter = radius * 2;
            var arc = new RectangleF(bounds.Location, new Size(diameter, diameter));

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, RectangleF bounds, int cornerRadius)
        {
            using (var path = RoundedRect(bounds, cornerRadius))
            {
                graphics.DrawPath(pen, path);
            }
        }

        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, RectangleF bounds, int cornerRadius)
        {
            using (var path = RoundedRect(bounds, cornerRadius))
            {
                graphics.FillPath(brush, path);
            }
        }

        public static ImageFormat ToDrawingImageFormat(this ImageFormats format)
        {
            switch (format)
            {
                case ImageFormats.Jpg:
                    return ImageFormat.Jpeg;

                case ImageFormats.Png:
                    return ImageFormat.Png;

                case ImageFormats.Gif:
                    return ImageFormat.Gif;

                case ImageFormats.Bmp:
                    return ImageFormat.Bmp;

                default:
                    return ImageFormat.Png;
            }
        }
    }
}