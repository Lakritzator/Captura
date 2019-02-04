using System.IO;

namespace Captura.Base.Images
{
    public interface IImagingSystem
    {
        IBitmapImage CreateBitmap(int width, int height);

        IBitmapImage LoadBitmap(string fileName);

        IBitmapImage LoadBitmap(Stream stream);
    }
}