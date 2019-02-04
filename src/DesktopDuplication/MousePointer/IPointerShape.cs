using System;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace DesktopDuplication.MousePointer
{
    public interface IPointerShape : IDisposable
    {
        void Update(Texture2D desktopTexture, OutputDuplicatePointerPosition pointerPosition);

        Bitmap GetBitmap();
    }
}