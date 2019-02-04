using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace DesktopDuplication.MousePointer
{
    public abstract class MaskedPointerShape : IPointerShape
    {
        private Direct2DEditorSession _editorSession;
        private Texture2D _copyTex;
        private Bitmap _bitmap;
        protected byte[] ShapeBuffer, DesktopBuffer;

        protected int Width { get; }
        protected int Height { get; }

        public MaskedPointerShape(int width, int height, Direct2DEditorSession editorSession)
        {
            Width = width;
            Height = height;
            _editorSession = editorSession;

            var copyTexDesc = new Texture2DDescription
            {
                Width = Width,
                Height = Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.B8G8R8A8_UNorm,
                SampleDescription =
                {
                    Count = 1,
                    Quality = 0
                },
                Usage = ResourceUsage.Staging,
                BindFlags = 0,
                CpuAccessFlags = CpuAccessFlags.Read
            };

            _copyTex = new Texture2D(_editorSession.Device, copyTexDesc);

            ShapeBuffer = new byte[width * height * 4];
            DesktopBuffer = new byte[width * height * 4];
        }

        public Bitmap GetBitmap() => _bitmap;

        public void Update(Texture2D desktopTexture, OutputDuplicatePointerPosition pointerPosition)
        {
            _bitmap?.Dispose();

            var region = new ResourceRegion(
                pointerPosition.Position.X,
                pointerPosition.Position.Y,
                0,
                pointerPosition.Position.X + Width,
                pointerPosition.Position.Y + Height,
                1);

            _editorSession.Device.ImmediateContext.CopySubresourceRegion(
                desktopTexture,
                0,
                region,
                _copyTex,
                0);

            var desktopMap = _editorSession.Device.ImmediateContext.MapSubresource(
                _copyTex,
                0,
                MapMode.Read,
                MapFlags.None);

            try
            {
                Marshal.Copy(desktopMap.DataPointer, DesktopBuffer, 0, DesktopBuffer.Length);
            }
            finally
            {
                _editorSession.Device.ImmediateContext.UnmapSubresource(_copyTex, 0);
            }

            OnUpdate();

            var gcPin = GCHandle.Alloc(ShapeBuffer, GCHandleType.Pinned);

            try
            {
                var pitch = Width * 4;

                _bitmap = new Bitmap(_editorSession.RenderTarget,
                    new Size2(Width, Height),
                    new DataPointer(gcPin.AddrOfPinnedObject(), Height * pitch),
                    pitch,
                    new BitmapProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
            }
            finally
            {
                gcPin.Free();
            }
        }

        protected abstract void OnUpdate();
        protected abstract void OnDispose();

        public void Dispose()
        {
            _bitmap?.Dispose();
            _bitmap = null;

            _copyTex?.Dispose();
            _copyTex = null;

            ShapeBuffer = DesktopBuffer = null;

            _editorSession = null;

            OnDispose();
        }
    }
}