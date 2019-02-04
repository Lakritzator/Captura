using System.Runtime.InteropServices;
using Captura.Base.Images;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace DesktopDuplication
{
    public class Texture2DFrame : IBitmapFrame
    {
        public Texture2D Texture { get; }
        public Texture2D PreviewTexture { get; }

        public Device Device { get; }

        public Texture2DFrame(Texture2D texture, Device device, Texture2D texture2D)
        {
            Texture = texture;
            Device = device;
            PreviewTexture = texture2D;

            var desc = texture.Description;

            Width = desc.Width;
            Height = desc.Height;
        }

        public void Dispose() { }

        public int Width { get; }
        public int Height { get; }

        public void CopyTo(byte[] buffer, int length)
        {
            var mapSource = Device.ImmediateContext.MapSubresource(Texture, 0, MapMode.Read, MapFlags.None);

            try
            {
                Marshal.Copy(mapSource.DataPointer, buffer, 0, length);
            }
            finally
            {
                Device.ImmediateContext.UnmapSubresource(Texture, 0);
            }
        }
    }
}