using System;
using System.Drawing;
using System.Drawing.Imaging;
using Captura.Base.Images;

namespace Screna.Frames
{
    public class GraphicsEditor : IEditableFrame
    {
        private readonly Bitmap _image;
        private readonly Graphics _graphics;

        public GraphicsEditor(Bitmap image)
        {
            _image = image;

            _graphics = Graphics.FromImage(image);
        }

        public IBitmapFrame GenerateFrame()
        {
            Dispose();

            return new OneTimeFrame(_image);
        }

        public IDisposable CreateBitmapBgr32(Size size, IntPtr memoryData, int stride)
        {
            return GraphicsBitmapLoader.Instance.CreateBitmapBgr32(size, memoryData, stride);
        }

        public IDisposable LoadBitmap(string fileName, out Size size)
        {
            return GraphicsBitmapLoader.Instance.LoadBitmap(fileName, out size);
        }

        public void DrawImage(object image, Rectangle? region, int opacity = 100)
        {
            if (!(image is Image img))
                return;

            var regionToUse = region ?? new Rectangle(Point.Empty, img.Size);

            if (opacity < 100)
            {
                var colorMatrix = new ColorMatrix
                {
                    Matrix33 = opacity / 100.0f
                };

                var imgAttribute = new ImageAttributes();
                imgAttribute.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                _graphics.DrawImage(img, regionToUse, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttribute);
            }
            else _graphics.DrawImage(img, regionToUse);
        }

        public void FillRectangle(Color color, RectangleF rectangle)
        {
            _graphics.FillRectangle(new SolidBrush(color), rectangle);
        }

        public void FillRectangle(Color color, RectangleF rectangle, int cornerRadius)
        {
            _graphics.FillRoundedRectangle(new SolidBrush(color), rectangle, cornerRadius);
        }

        public void FillEllipse(Color color, RectangleF rectangle)
        {
            _graphics.FillEllipse(new SolidBrush(color), rectangle);
        }

        public void DrawEllipse(Color color, float strokeWidth, RectangleF rectangle)
        {
            _graphics.DrawEllipse(new Pen(color, strokeWidth), rectangle);
        }

        public void DrawRectangle(Color color, float strokeWidth, RectangleF rectangle)
        {
            _graphics.DrawRectangle(new Pen(color, strokeWidth), rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public void DrawRectangle(Color color, float strokeWidth, RectangleF rectangle, int cornerRadius)
        {
            _graphics.DrawRoundedRectangle(new Pen(color, strokeWidth), rectangle, cornerRadius);
        }

        public SizeF MeasureString(string text, int fontSize)
        {
            var font = new Font(FontFamily.GenericMonospace, fontSize);

            return _graphics.MeasureString(text, font);
        }

        public void DrawString(string text, int fontSize, Color color, RectangleF layoutRectangle)
        {
            var font = new Font(FontFamily.GenericMonospace, fontSize);

            _graphics.DrawString(text, font, new SolidBrush(color), layoutRectangle);
        }

        public float Width => _graphics.VisibleClipBounds.Width;

        public float Height => _graphics.VisibleClipBounds.Height;

        public void Dispose()
        {
            _graphics.Dispose();
        }
    }
}