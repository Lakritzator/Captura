using System;
using System.Drawing;
using Captura.Base.Images;
using Captura.Native;
using Screna.Frames;

namespace Screna.ImageProviders
{
    public class RegionProvider : IImageProvider
    {
        private Rectangle _region;
        private readonly Func<Point> _locationFunc;
        private readonly bool _includeCursor;
        private readonly Func<Point, Point> _transform;

        private readonly IntPtr _hdcSrc, _hdcDest, _hBitmap;

        public RegionProvider(Rectangle region, bool includeCursor)
            : this(region, includeCursor, () => region.Location) { }

        public RegionProvider(Rectangle region, bool includeCursor, Func<Point> locationFunc)
        {
            _region = region;
            _includeCursor = includeCursor;
            _locationFunc = locationFunc;

            // Width and Height must be even.
            // Use these for Bitmap size, but capture as per region size
            Width = _region.Width;
            if (Width % 2 == 1)
                ++Width;
            
            Height = _region.Height;
            if (Height % 2 == 1)
                ++Height;

            _transform = point => new Point(point.X - _region.X, point.Y - _region.Y);

            _hdcSrc = User32.GetDC(IntPtr.Zero);

            _hdcDest = Gdi32.CreateCompatibleDC(_hdcSrc);
            _hBitmap = Gdi32.CreateCompatibleBitmap(_hdcSrc, Width, Height);

            Gdi32.SelectObject(_hdcDest, _hBitmap);
        }

        public void Dispose()
        {
            Gdi32.DeleteDC(_hdcDest);
            User32.ReleaseDC(IntPtr.Zero, _hdcSrc);
            Gdi32.DeleteObject(_hBitmap);
        }

        public IEditableFrame Capture()
        {
            // Update Location
            _region.Location = _locationFunc();

            Gdi32.BitBlt(_hdcDest, 0, 0, _region.Width, _region.Height,
                _hdcSrc, _region.X, _region.Y,
                (int) CopyPixelOperation.SourceCopy);

            var img = new GraphicsEditor(Image.FromHbitmap(_hBitmap));

            if (_includeCursor)
                MouseCursor.Draw(img, _transform);

            return img;
        }

        public int Height { get; }
        public int Width { get; }
    }
}
