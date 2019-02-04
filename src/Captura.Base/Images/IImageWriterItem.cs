using System.Threading.Tasks;

namespace Captura.Base.Images
{
    public interface IImageWriterItem
    {
        Task Save(IBitmapImage image, ImageFormats format, string fileName);

        string Display { get; }

        bool Active { get; set; }
    }
}
