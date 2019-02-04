using System;
using System.Drawing;
using System.Windows.Forms;
using Captura.Base;
using Captura.Base.Images;
using Captura.Native;
using Captura.Native.Structs;
using Screna.Frames;

namespace Screna.ImageProviders
{
    /// <summary>
    /// Captures the specified window.
    /// </summary>
    public class WindowProvider : IImageProvider
    {
        /// <summary>
        /// A <see cref="Rectangle"/> representing the entire Desktop.
        /// </summary>
        public static Rectangle DesktopRectangle => SystemInformation.VirtualScreen;

        private readonly IWindow _window;
        private readonly Func<Point, Point> _transform;
        private readonly bool _includeCursor;

        private readonly IntPtr _hdcSrc, _hdcDest, _hBitmap;

        private static Func<Point, Point> GetTransformer(IWindow window)
        {
            var initialSize = window.Rectangle.Even().Size;

            return point =>
            {
                var rect = window.Rectangle;
                
                var ratio = Math.Min((float)initialSize.Width / rect.Width, (float)initialSize.Height / rect.Height);

                return new Point((int)((point.X - rect.X) * ratio), (int)((point.Y - rect.Y) * ratio));
            };
        }
        
        /// <summary>
        /// Creates a new instance of <see cref="WindowProvider"/>.
        /// </summary>
        public WindowProvider(IWindow window, bool includeCursor, out Func<Point, Point> transform)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _includeCursor = includeCursor;

            var size = window.Rectangle.Even().Size;
            Width = size.Width;
            Height = size.Height;

            transform = _transform = GetTransformer(window);

            _hdcSrc = User32.GetDC(IntPtr.Zero);

            _hdcDest = Gdi32.CreateCompatibleDC(_hdcSrc);
            _hBitmap = Gdi32.CreateCompatibleBitmap(_hdcSrc, Width, Height);

            Gdi32.SelectObject(_hdcDest, _hBitmap);
        }

        private void OnCapture()
        {
            if (!_window.IsAlive)
            {
                throw new WindowClosedException();
            }

            var rect = _window.Rectangle.Even();
            var ratio = Math.Min((float) Width / rect.Width, (float) Height / rect.Height);

            var resizeWidth = (int) (rect.Width * ratio);
            var resizeHeight = (int) (rect.Height * ratio);

            void ClearRect(RECT rectToClear)
            {
                User32.FillRect(_hdcDest, ref rectToClear, IntPtr.Zero);
            }

            if (Width != resizeWidth)
            {
                ClearRect(new RECT
                {
                    Left = resizeWidth,
                    Right = Width,
                    Bottom = Height
                });
            }
            else if (Height != resizeHeight)
            {
                ClearRect(new RECT
                {
                    Top = resizeHeight,
                    Right = Width,
                    Bottom = Height
                });
            }

            Gdi32.StretchBlt(_hdcDest, 0, 0, resizeWidth, resizeHeight,
                _hdcSrc, rect.X, rect.Y, rect.Width, rect.Height,
                (int) CopyPixelOperation.SourceCopy);
        }

        public IEditableFrame Capture()
        {
            try
            {
                OnCapture();

                var img = new GraphicsEditor(Image.FromHbitmap(_hBitmap));

                if (_includeCursor)
                    MouseCursor.Draw(img, _transform);

                return img;
            }
            catch (Exception e) when (!(e is WindowClosedException))
            {
                return RepeatFrame.Instance;
            }
        }

        /// <summary>
        /// Height of Captured image.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Width of Captured image.
        /// </summary>
        public int Width { get; }

        public void Dispose()
        {
            Gdi32.DeleteDC(_hdcDest);
            User32.ReleaseDC(IntPtr.Zero, _hdcSrc);
            Gdi32.DeleteObject(_hBitmap);
        }
    }
}
