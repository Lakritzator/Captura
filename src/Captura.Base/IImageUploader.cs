using System;
using System.Threading.Tasks;
using Captura.Base.Images;

namespace Captura.Base
{
    public interface IImageUploader
    {
        Task<UploadResult> Upload(IBitmapImage image, ImageFormats format, Action<int> progress);

        Task DeleteUploadedFile(string deleteHash);

        string UploadServiceName { get; }
    }
}