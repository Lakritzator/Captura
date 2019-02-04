// Adapted from https://github.com/jasonpang/desktop-duplication-net

using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using Captura.Base.Images;
using DesktopDuplication.MousePointer;
using SharpDX.Direct3D;
using Device = SharpDX.Direct3D11.Device;

namespace DesktopDuplication
{
    public class DesktopDuplicator : IDisposable
    {
        private Texture2D _desktopImageTexture;
        private Texture2D _stagingTexture;
        private Direct2DEditorSession _editorSession;
        private DxMousePointer _mousePointer;
        private DesktopDuplicationCapture _desktopDuplicationCapture;
        private Device _device;
        private Device _deviceForDesktopDuplication;

        public DesktopDuplicator(bool includeCursor, Output1 output)
        {
            _device = new Device(DriverType.Hardware,
                DeviceCreationFlags.BgraSupport,
                FeatureLevel.Level_11_1);

            // Separate Device required otherwise AccessViolationException happens
            using (var adapter = output.GetParent<Adapter>())
                _deviceForDesktopDuplication = new Device(adapter);

            _desktopDuplicationCapture = new DesktopDuplicationCapture(_deviceForDesktopDuplication, output);

            var bounds = output.Description.DesktopBounds;
            var width = bounds.Right - bounds.Left;
            var height = bounds.Bottom - bounds.Top;

            _stagingTexture = new Texture2D(_device, new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            });

            _desktopImageTexture = new Texture2D(_device, new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.None,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Default
            });

            _editorSession = new Direct2DEditorSession(_desktopImageTexture, _device, _stagingTexture);

            if (includeCursor)
                _mousePointer = new DxMousePointer(_editorSession);
        }

        public IEditableFrame Capture()
        {
            try
            {
                if (!_desktopDuplicationCapture.Get(_desktopImageTexture, _mousePointer))
                {
                    return RepeatFrame.Instance;
                }
            }
            catch
            {
                try
                {
                    _desktopDuplicationCapture.Init();
                }
                catch
                {
                    // ignored
                }

                return RepeatFrame.Instance;
            }

            var editor = new Direct2DEditor(_editorSession);

            _mousePointer?.Draw(editor);

            return editor;
        }

        public void Dispose()
        {
            try
            {
                _mousePointer?.Dispose();
            }
            catch
            {
                // Ignored in dispose
            }
            _mousePointer = null;

            try { _editorSession.Dispose(); }
            catch
            {
                // Ignored in dispose
            }

            _editorSession = null;

            try { _desktopDuplicationCapture.Dispose(); }
            catch
            {
                // Ignored in dispose
            }

            _desktopDuplicationCapture = null;

            try { _desktopImageTexture.Dispose(); }
            catch
            {
                // Ignored in dispose
            }
            _desktopImageTexture = null;

            try { _stagingTexture.Dispose(); }
            catch
            {
                // Ignored in dispose
            }
            _stagingTexture = null;

            try { _device.Dispose(); }
            catch
            {
                // Ignored in dispose
            }
            _device = null;

            try { _deviceForDesktopDuplication.Dispose(); }
            catch
            {
                // Ignored in dispose
            }
            _deviceForDesktopDuplication = null;
        }
    }
}
