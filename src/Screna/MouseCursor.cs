using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Captura.Base.Images;
using Captura.Native;
using Captura.Native.Structs;

namespace Screna
{
    /// <summary>
    /// Draws the MouseCursor on an Image
    /// </summary>
    public static class MouseCursor
    {
        private const int CursorShowing = 1;
                
        /// <summary>
        /// Gets the Current Mouse Cursor Position.
        /// </summary>
        public static Point CursorPosition
        {
            get
            {
                var p = new Point();
                User32.GetCursorPos(ref p);
                return p;
            }
        }

        // hCursor -> (Icon, Hotspot)
        private static readonly Dictionary<IntPtr, Tuple<Bitmap, Point>> Cursors = new Dictionary<IntPtr, Tuple<Bitmap, Point>>();
        
        /// <summary>
        /// Draws this overlay.
        /// </summary>
        /// <param name="graphics">A <see cref="Graphics"/> object to draw upon.</param>
        /// <param name="transform">Point Transform Function.</param>
        public static void Draw(Graphics graphics, Func<Point, Point> transform = null)
        {
            GetIcon(transform, out var icon, out var location);

            if (icon == null)
                return;

            try
            {
                graphics.DrawImage(icon, new Rectangle(location, icon.Size));
            }
            catch (ArgumentException) { }
        }

        public static void Draw(IEditableFrame editableFrame, Func<Point, Point> transform = null)
        {
            GetIcon(transform, out var icon, out var location);

            if (icon == null)
                return;

            try
            {
                editableFrame.DrawImage(icon, new Rectangle(location, icon.Size));
            }
            catch (ArgumentException) { }
        }

        private static void GetIcon(Func<Point, Point> transform, out Bitmap icon, out Point location)
        {
            icon = null;
            location = Point.Empty;

            // ReSharper disable once RedundantAssignment
            // ReSharper disable once InlineOutVariableDeclaration
            var cursorInfo = new CursorInfo {cbSize = Marshal.SizeOf<CursorInfo>()};

            if (!User32.GetCursorInfo(out cursorInfo))
                return;

            if (cursorInfo.flags != CursorShowing)
                return;

            Point hotspot;

            if (Cursors.ContainsKey(cursorInfo.hCursor))
            {
                var tuple = Cursors[cursorInfo.hCursor];

                icon = tuple.Item1;
                hotspot = tuple.Item2;
            }
            else
            {
                var hIcon = User32.CopyIcon(cursorInfo.hCursor);

                if (hIcon == IntPtr.Zero)
                    return;

                if (!User32.GetIconInfo(hIcon, out var icInfo))
                    return;

                icon = Icon.FromHandle(hIcon).ToBitmap();
                hotspot = new Point(icInfo.xHotspot, icInfo.yHotspot);

                Cursors.Add(cursorInfo.hCursor, Tuple.Create(icon, hotspot));

                User32.DestroyIcon(hIcon);

                Gdi32.DeleteObject(icInfo.hbmColor);
                Gdi32.DeleteObject(icInfo.hbmMask);
            }

            location = new Point(cursorInfo.ptScreenPos.X - hotspot.X,
                cursorInfo.ptScreenPos.Y - hotspot.Y);

            if (transform != null)
                location = transform(location);
        }
    }
}