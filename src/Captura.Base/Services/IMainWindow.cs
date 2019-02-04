namespace Captura.Base.Services
{
    public interface IMainWindow
    {
        bool IsVisible { get; set; }

        bool IsMinimized { get; set; }

        void EditImage(string fileName);

        void CropImage(string fileName);

        void TrimMedia(string fileName);

        void UploadToYouTube(string fileName);
    }
}
