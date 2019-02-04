using System;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.DirectWrite.Factory;
using Factory1 = SharpDX.Direct2D1.Factory1;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

namespace DesktopDuplication
{
    public class Direct2DEditorSession : IDisposable
    {
        private Texture2D _texture;

        public Device Device { get; private set; }
        public Texture2D StagingTexture { get; private set; }
        public RenderTarget RenderTarget { get; private set; }
        public Texture2D PreviewTexture { get; private set; }

        private SolidColorBrush _solidColorBrush;
        private Factory1 _factory;
        private Factory _writeFactory;
        private ImagingFactory _imagingFactory;

        public Factory WriteFactory => _writeFactory ?? (_writeFactory = new Factory());

        public ImagingFactory ImagingFactory => _imagingFactory ?? (_imagingFactory = new ImagingFactory());

        public SolidColorBrush GetSolidColorBrush(RawColor4 color)
        {
            if (_solidColorBrush == null)
            {
                _solidColorBrush = new SolidColorBrush(RenderTarget, color);
            }
            else _solidColorBrush.Color = color;

            return _solidColorBrush;
        }

        public Direct2DEditorSession(Texture2D texture, Device device, Texture2D stagingTexture)
        {
            _texture = texture;
            Device = device;
            StagingTexture = stagingTexture;

            var desc = texture.Description;
            desc.OptionFlags = ResourceOptionFlags.Shared;

            PreviewTexture = new Texture2D(device, desc);

            _factory = new Factory1(FactoryType.MultiThreaded);

            var pixelFormat = new PixelFormat(Format.Unknown, AlphaMode.Ignore);

            var renderTargetProps = new RenderTargetProperties(pixelFormat)
            {
                Type = RenderTargetType.Hardware
            };

            using (var surface = texture.QueryInterface<Surface>())
            {
                RenderTarget = new RenderTarget(_factory, surface, renderTargetProps);
            }
        }

        public void BeginDraw()
        {
            RenderTarget.BeginDraw();
        }

        public void EndDraw()
        {
            RenderTarget.EndDraw();
            Device.ImmediateContext.CopyResource(_texture, StagingTexture);
            Device.ImmediateContext.CopyResource(StagingTexture, PreviewTexture);
        }

        public void Dispose()
        {
            _texture = null;
            Device = null;
            StagingTexture = null;

            _solidColorBrush?.Dispose();
            _solidColorBrush = null;

            RenderTarget.Dispose();
            RenderTarget = null;

            _factory.Dispose();
            _factory = null;

            _writeFactory?.Dispose();
            _writeFactory = null;

            _imagingFactory?.Dispose();
            _imagingFactory = null;

            PreviewTexture.Dispose();
            PreviewTexture = null;
        }
    }
}