using System;
using System.Drawing;
using Captura.Base;
using Captura.Base.Audio;
using Captura.Base.Audio.WaveFormat;
using Captura.Base.Images;
using Captura.Base.Video;
using Moq;
using Screna.Frames;

namespace Tests.Fixtures
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MoqFixture : IDisposable
    {
        public void Dispose() { }

        public Mock<IImageProvider> GetImageProviderMock(int width = 100, int height = 50)
        {
            var mock = new Mock<IImageProvider>();

            mock.Setup(imageProvider => imageProvider.Width).Returns(width);

            mock.Setup(imageProvider => imageProvider.Height).Returns(height);

            mock.Setup(imageProvider => imageProvider.Capture()).Returns(new GraphicsEditor(new Bitmap(width, height)));

            return mock;
        }

        public Mock<IAudioProvider> GetAudioProviderMock()
        {
            var mock = new Mock<IAudioProvider>();

            mock.Setup(audioProvider => audioProvider.WaveFormat).Returns(new WaveFormat());

            return mock;
        }

        public Mock<IAudioFileWriter> GetAudioFileWriterMock()
        {
            return new Mock<IAudioFileWriter>();
        }

        public Mock<IVideoFileWriter> GetVideoFileWriterMock()
        {
            var mock = new Mock<IVideoFileWriter>();

            mock.Setup(videoFileWriter => videoFileWriter.WriteFrame(It.IsAny<IBitmapFrame>()))
                .Callback<IBitmapFrame>(bitmapFrame => bitmapFrame.Dispose());

            mock.Setup(videoFileWriter => videoFileWriter.SupportsAudio).Returns(true);

            return mock;
        }

        public Mock<IOverlay> GetOverlayMock()
        {
            return new Mock<IOverlay>();
        }
    }
}