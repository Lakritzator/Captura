using System;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;

namespace DesktopDuplication.MousePointer
{
    public class ColorPointerShape : IPointerShape
    {
        private Bitmap _bitmap;

        public ColorPointerShape(IntPtr shapeBuffer,
            OutputDuplicatePointerShapeInformation shapeInfo,
            RenderTarget renderTarget)
        {
            _bitmap = new Bitmap(renderTarget,
                new Size2(shapeInfo.Width, shapeInfo.Height),
                new DataPointer(shapeBuffer, shapeInfo.Height * shapeInfo.Pitch),
                shapeInfo.Pitch,
                new BitmapProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
        }

        public void Update(Texture2D desktopTexture, OutputDuplicatePointerPosition pointerPosition) { }

        public Bitmap GetBitmap() => _bitmap;

        public void Dispose()
        {
            _bitmap.Dispose();
            _bitmap = null;
        }
    }
}