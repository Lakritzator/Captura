using System;

namespace Captura.Base.Audio
{
    /// <summary>
    /// Encodes Audio into an audio file.
    /// </summary>
    public interface IAudioFileWriter : IDisposable
    {
        /// <summary>
        /// Writes to file.
        /// </summary>
        void Write(byte[] data, int offset, int count);

        /// <summary>
        /// Writes all buffered data to file.
        /// </summary>
        void Flush();
    }
}
