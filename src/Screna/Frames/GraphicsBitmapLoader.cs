using System;
using System.Drawing;
using System.Drawing.Imaging;
using Captura.Base.Images;

namespace Screna.Frames
{
    public class GraphicsBitmapLoader : IBitmapLoader
    {
        private GraphicsBitmapLoader() { }

        public static GraphicsBitmapLoader Instance { get; } = new GraphicsBitmapLoader();

        public IDisposable CreateBitmapBgr32(Size size, IntPtr memoryData, int stride)
        {
            return new Bitmap(size.Width, size.Height, stride, PixelFormat.Format32bppRgb, memoryData);
        }

        public IDisposable LoadBitmap(string fileName, out Size size)
        {
            var bmp = new Bitmap(fileName);

            size = bmp.Size;

            return bmp;
        }

        public void Dispose() { }
    }
}