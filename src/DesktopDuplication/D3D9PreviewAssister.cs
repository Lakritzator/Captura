using System;
using Captura.Base.Services;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;

// Adapted from https://github.com/Marlamin/SharpDX.WPF

namespace DesktopDuplication
{
    public class D3D9PreviewAssister : IDisposable
    {
        private readonly Direct3DEx _direct3D;
        private readonly DeviceEx _device;

        public D3D9PreviewAssister(IPlatformServices platformServices)
        {
            _direct3D = new Direct3DEx();

            var presentParameters = new PresentParameters
            {
                Windowed = true,
                SwapEffect = SwapEffect.Discard,
                DeviceWindowHandle = platformServices.DesktopWindow.Handle,
                PresentationInterval = PresentInterval.Default
            };

            _device = new DeviceEx(_direct3D,
                0,
                DeviceType.Hardware,
                IntPtr.Zero,
                CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve,
                presentParameters);
        }

        public Texture GetSharedTexture(Texture2D texture)
        {
            return GetSharedD3D9(_device, texture);
        }

        // Texture must be created with ResourceOptionFlags.Shared
        // Texture format must be B8G8R8A8_UNorm
        private static Texture GetSharedD3D9(DeviceEx device, Texture2D renderTarget)
        {
            using (var resource = renderTarget.QueryInterface<SharpDX.DXGI.Resource>())
            {
                var handle = resource.SharedHandle;

                if (handle == IntPtr.Zero)
                    throw new ArgumentNullException(nameof(handle));

                return new Texture(device,
                    renderTarget.Description.Width,
                    renderTarget.Description.Height,
                    1,
                    Usage.RenderTarget,
                    Format.A8R8G8B8,
                    Pool.Default,
                    ref handle);
            }
        }

        public void Dispose()
        {
            _device.Dispose();
            _direct3D.Dispose();
        }
    }
}