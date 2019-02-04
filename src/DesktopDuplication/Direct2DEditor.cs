using System;
using System.Drawing;
using Captura.Base.Images;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using BitmapInterpolationMode = SharpDX.Direct2D1.BitmapInterpolationMode;
using Color = System.Drawing.Color;
using PixelFormat = SharpDX.WIC.PixelFormat;
using Rectangle = System.Drawing.Rectangle;
using RectangleF = System.Drawing.RectangleF;

namespace DesktopDuplication
{
    public class Direct2DEditor : IEditableFrame
    {
        private readonly Direct2DEditorSession _editorSession;

        public Direct2DEditor(Direct2DEditorSession editorSession)
        {
            _editorSession = editorSession;

            var desc = editorSession.StagingTexture.Description;

            Width = desc.Width;
            Height = desc.Height;

            editorSession.BeginDraw();
        }

        public void Dispose() { }

        public float Width { get; }
        public float Height { get; }

        public IDisposable CreateBitmapBgr32(Size size, IntPtr memoryData, int stride)
        {
            return new Bitmap(_editorSession.RenderTarget,
                new Size2(size.Width, size.Height),
                new DataPointer(memoryData, size.Height * stride),
                stride,
                new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore)));
        }

        public IDisposable LoadBitmap(string fileName, out Size size)
        {
            using (var decoder = new BitmapDecoder(_editorSession.ImagingFactory, fileName, 0))
            using (var bmpSource = decoder.GetFrame(0))
            using (var convertedBmp = new FormatConverter(_editorSession.ImagingFactory))
            {
                convertedBmp.Initialize(bmpSource, PixelFormat.Format32bppPBGRA);

                size = new Size(bmpSource.Size.Width, bmpSource.Size.Height);

                return Bitmap.FromWicBitmap(_editorSession.RenderTarget, convertedBmp);
            }
        }

        public void DrawImage(object image, Rectangle? region, int opacity = 100)
        {
            if (!(image is Bitmap bmp))
            {
                return;
            }

            var rect = region ?? new RectangleF(0, 0, bmp.Size.Width, bmp.Size.Height);
            var rawRect = new RawRectangleF(rect.Left,
                rect.Top,
                rect.Right,
                rect.Bottom);

            _editorSession.RenderTarget.DrawBitmap(bmp, rawRect, opacity, BitmapInterpolationMode.Linear);
        }

        SolidColorBrush Convert(Color color)
        {
            var solidColor = new RawColor4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

            return _editorSession.GetSolidColorBrush(solidColor);
        }

        RawRectangleF Convert(RectangleF rectangle)
        {
            return new RawRectangleF(rectangle.Left,
                rectangle.Top,
                rectangle.Right,
                rectangle.Bottom);
        }

        RoundedRectangle Convert(RectangleF rectangle, int cornerRadius)
        {
            return new RoundedRectangle
            {
                Rect = Convert(rectangle),
                RadiusX = cornerRadius,
                RadiusY = cornerRadius
            };
        }

        Ellipse ToEllipse(RectangleF rectangle)
        {
            var center = new RawVector2(rectangle.Left + rectangle.Width / 2f,
                rectangle.Top + rectangle.Height / 2f);

            return new Ellipse(center,
                rectangle.Width / 2f,
                rectangle.Height / 2f);
        }

        public void FillRectangle(Color color, RectangleF rectangle)
        {
            _editorSession.RenderTarget.FillRectangle(Convert(rectangle), Convert(color));
        }

        public void FillRectangle(Color color, RectangleF rectangle, int cornerRadius)
        {
            _editorSession.RenderTarget.FillRoundedRectangle(Convert(rectangle, cornerRadius), Convert(color));
        }

        public void DrawRectangle(Color color, float strokeWidth, RectangleF rectangle)
        {
            _editorSession.RenderTarget.DrawRectangle(Convert(rectangle), Convert(color), strokeWidth);
        }

        public void DrawRectangle(Color color, float strokeWidth, RectangleF rectangle, int cornerRadius)
        {
            _editorSession.RenderTarget.DrawRoundedRectangle(Convert(rectangle, cornerRadius), Convert(color), strokeWidth);
        }

        public void FillEllipse(Color color, RectangleF rectangle)
        {
            _editorSession.RenderTarget.FillEllipse(ToEllipse(rectangle), Convert(color));
        }

        public void DrawEllipse(Color color, float strokeWidth, RectangleF rectangle)
        {
            _editorSession.RenderTarget.DrawEllipse(ToEllipse(rectangle), Convert(color), strokeWidth);
        }

        TextFormat GetTextFormat(int fontSize)
        {
            return new TextFormat(_editorSession.WriteFactory, "Arial", fontSize);
        }

        TextLayout GetTextLayout(string text, TextFormat format)
        {
            return new TextLayout(_editorSession.WriteFactory, text, format, Width, Height);
        }

        public SizeF MeasureString(string text, int fontSize)
        {
            using (var format = GetTextFormat(fontSize))
            using (var layout = GetTextLayout(text, format))
            {
                return new SizeF(layout.Metrics.Width, layout.Metrics.Height);
            }
        }

        public void DrawString(string text, int fontSize, Color color, RectangleF layoutRectangle)
        {
            using (var format = GetTextFormat(fontSize))
            using (var layout = GetTextLayout(text, format))
            {
                _editorSession.RenderTarget.DrawTextLayout(
                    new RawVector2(layoutRectangle.X, layoutRectangle.Y),
                    layout,
                    Convert(color));
            }
        }

        public IBitmapFrame GenerateFrame()
        {
            _editorSession.EndDraw();

            return new Texture2DFrame(_editorSession.StagingTexture, _editorSession.Device, _editorSession.PreviewTexture);
        }
    }
}