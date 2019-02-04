using Captura.Base.Images;
using Captura.Base.Services;

namespace Captura.Fakes
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FakePreviewWindow : IPreviewWindow
    {
        public void Dispose() { }

        public void Init(int width, int height) { }

        public void Display(IBitmapFrame frame)
        {
            frame.Dispose();
        }

        public void Show() { }
    }
}