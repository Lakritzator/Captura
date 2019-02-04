using System.Drawing;
using Captura.Base.Images;
using Captura.Base.Services;
using Screna.Frames;

namespace Captura.WebCam
{
    public class WebCamImageProvider : IImageProvider
    {
        private readonly IWebCamProvider _webCamProvider;

        public WebCamImageProvider(IWebCamProvider webCamProvider)
        {
            _webCamProvider = webCamProvider;
        }

        public void Dispose() { }

        public IEditableFrame Capture()
        {
            try
            {
                var capture = _webCamProvider.Capture(GraphicsBitmapLoader.Instance);

                if (capture is Bitmap bitmap)
                {
                    return new GraphicsEditor(bitmap);
                }

                return RepeatFrame.Instance;
            }
            catch
            {
                return RepeatFrame.Instance;
            }
        }

        public int Height => _webCamProvider.Height;

        public int Width => _webCamProvider.Width;
    }
}