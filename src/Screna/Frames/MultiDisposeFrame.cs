using System;
using Captura.Base.Images;

namespace Screna.Frames
{
    public class MultiDisposeFrame : IBitmapFrame
    {
        private int _count;

        public IBitmapFrame Frame { get; }

        private readonly object _syncLock = new object();

        public MultiDisposeFrame(IBitmapFrame frame, int count)
        {
            if (Frame is RepeatFrame)
            {
                throw new NotSupportedException();
            }

            if (count < 2)
            {
                throw new ArgumentException("Count should be at least 2", nameof(count));
            }

            Frame = frame ?? throw new ArgumentNullException(nameof(frame));
            _count = count;
        }

        public void Dispose()
        {
            lock (_syncLock)
            {
                --_count;

                if (_count == 0)
                {
                    Frame.Dispose();
                }
            }
        }

        public int Width => Frame.Width;
        public int Height => Frame.Height;

        public void CopyTo(byte[] buffer, int length)
        {
            Frame.CopyTo(buffer, length);
        }
    }
}