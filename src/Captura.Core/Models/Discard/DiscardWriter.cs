using Captura.Base.Images;
using Captura.Base.Video;

namespace Captura.Core.Models.Discard
{
    public class DiscardWriter : IVideoFileWriter
    {
        public void Dispose() { }

        readonly byte[] _dummyBuffer = { 0 };

        public void WriteFrame(IBitmapFrame image)
        {
            if (image is RepeatFrame)
                return;

            using (image)
            {
                // HACK: Don't know why, this fixes Preview showing multiple mouse pointers
                image.CopyTo(_dummyBuffer, _dummyBuffer.Length);
            }
        }

        public bool SupportsAudio => false;

        public void WriteAudio(byte[] buffer, int length) { }
    }
}