using System.Drawing;
using System.IO;
using Captura.Base.Images;

namespace Screna.Frames
{
    public class DrawingImage : IBitmapImage
    {
        public Image Image { get; }

        public DrawingImage(Image image)
        {
            Image = image;
        }

        public void Dispose()
        {
            Image.Dispose();
        }

        public int Width => Image.Width;
        public int Height => Image.Height;

        public void Save(string fileName, ImageFormats format)
        {
            Image.Save(fileName, format.ToDrawingImageFormat());
        }

        public void Save(Stream stream, ImageFormats format)
        {
            Image.Save(stream, format.ToDrawingImageFormat());
        }
    }
}