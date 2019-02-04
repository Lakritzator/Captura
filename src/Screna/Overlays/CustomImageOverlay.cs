using System;
using System.Drawing;
using Captura.Base.Images;
using Screna.Overlays.Settings;

namespace Screna.Overlays
{
    public class CustomImageOverlay : ImageOverlay<CustomImageOverlaySettings>
    {
        private IDisposable _bmp;
        private Size _size;

        public CustomImageOverlay(CustomImageOverlaySettings imageOverlaySettings)
            : base(imageOverlaySettings, false) { }

        public override void Dispose()
        {
            _bmp?.Dispose();
        }

        protected override IDisposable GetImage(IEditableFrame editor, out Size size)
        {
            if (_bmp == null)
            {
                _bmp = editor.LoadBitmap(Settings.Source, out _size);
            }

            size = _size;

            return _bmp;
        }
    }
}