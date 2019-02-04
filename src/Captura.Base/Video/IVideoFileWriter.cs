using System;
using Captura.Base.Images;

namespace Captura.Base.Video
{
    /// <summary>
    /// Creates a video from individual frames and writes them to a file.
    /// </summary>
    public interface IVideoFileWriter : IDisposable
    {
        /// <summary>
        /// Writes an Image frame.
        /// </summary>
        /// <param name="image">The Image frame to write.</param>
        void WriteFrame(IBitmapFrame image);
        
        /// <summary>
        /// Gets whether audio is supported.
        /// </summary>
        bool SupportsAudio { get; }
                
        /// <summary>
        /// Write audio block to Audio Stream.
        /// </summary>
        /// <param name="buffer">Buffer containing audio data.</param>
        /// <param name="length">Length of audio data in bytes.</param>
        void WriteAudio(byte[] buffer, int length);
    }
}
