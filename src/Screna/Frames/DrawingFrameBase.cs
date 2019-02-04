using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Captura.Base.Images;

namespace Screna.Frames
{
    public abstract class DrawingFrameBase : IBitmapFrame
    {
        public Bitmap Bitmap { get; }

        protected DrawingFrameBase(Bitmap bitmap)
        {
            Bitmap = bitmap;
            Width = bitmap.Width;
            Height = bitmap.Height;
        }

        public abstract void Dispose();

        public void SaveGif(Stream stream)
        {
            Bitmap.Save(stream, ImageFormat.Gif);
        }

        public int Width { get; }
        public int Height { get; }

        public void CopyTo(byte[] buffer, int length)
        {
            var bits = Bitmap.LockBits(new Rectangle(Point.Empty, Bitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                Marshal.Copy(bits.Scan0, buffer, 0, length);
            }
            finally
            {
                Bitmap.UnlockBits(bits);
            }
        }
    }
}