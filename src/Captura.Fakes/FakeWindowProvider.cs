using Captura.Base.Services;

namespace Captura.Fakes
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class FakeWindowProvider : IMainWindow
    {
        public bool IsVisible
        {
            get => true;
            set { }
        }

        public bool IsMinimized
        {
            get => false;
            set { }
        }

        public void EditImage(string fileName) { }

        public void CropImage(string fileName) { }

        public void TrimMedia(string fileName) { }

        public void UploadToYouTube(string fileName) { }
    }
}
