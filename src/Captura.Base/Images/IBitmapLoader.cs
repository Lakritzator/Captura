using System;
using System.Drawing;

namespace Captura.Base.Images
{
    public interface IBitmapLoader : IDisposable
    {
        IDisposable CreateBitmapBgr32(Size size, IntPtr memoryData, int stride);

        IDisposable LoadBitmap(string fileName, out Size size);
    }
}