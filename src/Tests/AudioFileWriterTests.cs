using System;
using System.IO;
using Captura.Base.Audio.WaveFormat;
using Screna;
using Xunit;

namespace Tests
{
    [Collection(nameof(Tests))]
    public class AudioFileWriterTests
    {
        [Fact]
        public void NullAudioOutputStream()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                using (new AudioFileWriter(outStream: null, format: new WaveFormat())) { }
            });
        }

        [Fact]
        public void NullWaveFormat()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                using (new AudioFileWriter(Stream.Null, null)) { }
            });
        }

        [Fact]
        public void NullFileName()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                using (new AudioFileWriter(fileName: null, format: new WaveFormat())) { }
            });
        }
    }
}