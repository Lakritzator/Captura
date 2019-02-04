using System;
using System.IO;

namespace Captura.Base.Images
{
    public interface IBitmapImage : IDisposable
    {
        int Width { get; }

        int Height { get; }

        void Save(string fileName, ImageFormats format);

        void Save(Stream stream, ImageFormats format);

        // Assume 32bpp rgba
        //IntPtr Map(bool Read, bool Write);

        //void Unmap();
    }
}