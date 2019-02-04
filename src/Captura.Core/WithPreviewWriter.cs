using System.Threading.Tasks;
using Captura.Base.Images;
using Captura.Base.Services;
using Captura.Base.Video;
using Screna.Frames;

namespace Captura.Core
{
    public class WithPreviewWriter : IVideoFileWriter
    {
        readonly IPreviewWindow _preview;
        public IVideoFileWriter OriginalWriter { get; private set; }

        public WithPreviewWriter(IVideoFileWriter writer, IPreviewWindow preview)
        {
            OriginalWriter = writer;
            _preview = preview;
        }

        public void Dispose()
        {
            OriginalWriter.Dispose();
            OriginalWriter = null;
            _preview.Dispose();
        }

        public void WriteFrame(IBitmapFrame image)
        {
            if (image is RepeatFrame)
            {
                OriginalWriter.WriteFrame(image);
            }
            else
            {
                var frame = new MultiDisposeFrame(image, 2);

                OriginalWriter.WriteFrame(frame);
                Task.Run(() => _preview.Display(frame));
            }
        }

        public bool SupportsAudio => OriginalWriter.SupportsAudio;

        public void WriteAudio(byte[] buffer, int length)
        {
            OriginalWriter.WriteAudio(buffer, length);
        }
    }
}