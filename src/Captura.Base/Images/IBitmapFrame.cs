using System;

namespace Captura.Base.Images
{
    public interface IBitmapFrame : IDisposable
    {
        int Width { get; }

        int Height { get; }

        void CopyTo(byte[] buffer, int length);
    }
}