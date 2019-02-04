using System;
using System.Drawing;
using Captura.Base.Images;
using Captura.Base.Services;
using Captura.Core.Settings.Models;
using Screna.Overlays;

namespace Captura.Core
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WebCamOverlay : ImageOverlay<WebCamOverlaySettings>
    {
        readonly IWebCamProvider _webCamProvider;

        public WebCamOverlay(IWebCamProvider webCamProvider, Settings.Settings settings) : base(settings.WebCamOverlay, true)
        {
            _webCamProvider = webCamProvider;
        }

        protected override IDisposable GetImage(IEditableFrame editor, out Size size)
        {
            size = new Size(_webCamProvider.Width, _webCamProvider.Height);

            // No WebCam
            if (_webCamProvider.AvailableCams.Count < 1 || _webCamProvider.SelectedCam == _webCamProvider.AvailableCams[0])
                return null;

            return _webCamProvider.Capture(editor);
        }
    }
}