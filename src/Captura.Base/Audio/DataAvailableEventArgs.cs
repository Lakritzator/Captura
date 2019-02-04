using System;

namespace Captura.Base.Audio
{
    /// <summary>
    /// Data Available Event Args.
    /// </summary>
    public class DataAvailableEventArgs : EventArgs
    {
        /// <summary>
        /// Data Buffer.
        /// </summary>
        public byte[] Buffer { get; }

        /// <summary>
        /// Data Buffer Length.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Creates a new instance of <see cref="DataAvailableEventArgs"/>.
        /// </summary>
        public DataAvailableEventArgs(byte[] buffer, int length)
        {
            Buffer = buffer;
            Length = length;
        }
    }
}