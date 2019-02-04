using System.Drawing;
using System.IO;
using Captura.Base.Images;

namespace Screna.Frames
{
    public class DrawingImagingSystem : IImagingSystem
    {
        public IBitmapImage CreateBitmap(int width, int height)
        {
            return new DrawingImage(new Bitmap(width, height));
        }

        public IBitmapImage LoadBitmap(string fileName)
        {
            return new DrawingImage(new Bitmap(fileName));
        }

        public IBitmapImage LoadBitmap(Stream stream)
        {
            return new DrawingImage(new Bitmap(stream));
        }
    }
}