﻿using System;
using Moq;
using Screna.Recorder;
using Tests.Fixtures;
using Xunit;

namespace Tests
{
    [Collection(nameof(Tests))]
    public class RecorderTests
    {
        readonly MoqFixture _moq;

        public RecorderTests(MoqFixture moq)
        {
            _moq = moq;
        }

        [Fact]
        public void NullVideoWriter()
        {
            var imageProvider = _moq.GetImageProviderMock().Object;

            Assert.Throws<ArgumentNullException>(() =>
            {
                using (new Recorder(null, imageProvider, 10)) { }
            });
        }

        [Fact]
        public void NullImageProvider()
        {
            var videoWriter = _moq.GetVideoFileWriterMock().Object;

            Assert.Throws<ArgumentNullException>(() =>
            {
                using (new Recorder(videoWriter, null, 10)) { }
            });
        }

        [Fact]
        public void NullAudioWriter()
        {
            var audioProvider = _moq.GetAudioProviderMock().Object;

            Assert.Throws<ArgumentNullException>(() =>
            {
                using (new Recorder(null, audioProvider)) { }
            });
        }

        [Fact]
        public void NullAudioProvider()
        {
            var audioWriter = _moq.GetAudioFileWriterMock().Object;

            Assert.Throws<ArgumentNullException>(() =>
            {
                using (new Recorder(audioWriter, null)) { }
            });
        }

        [Fact]
        public void NullGifWriter()
        {
            var imageProvider = _moq.GetImageProviderMock().Object;

            Assert.Throws<ArgumentNullException>(() =>
            {
                using (new VfrGifRecorder(null, imageProvider)) { }
            });
        }

        [Fact]
        public void NegativeFrameRate()
        {
            var imageProvider = _moq.GetImageProviderMock().Object;
            var videoWriter = _moq.GetVideoFileWriterMock().Object;

            Assert.Throws<ArgumentException>(() =>
            {
                using (new Recorder(videoWriter, imageProvider, -1)) { }
            });
        }

        [Fact]
        public void ZeroFrameRate()
        {
            var imageProvider = _moq.GetImageProviderMock().Object;
            var videoWriter = _moq.GetVideoFileWriterMock().Object;

            Assert.Throws<ArgumentException>(() =>
            {
                using (new Recorder(videoWriter, imageProvider, 0)) { }
            });
        }

        [Fact]
        public void RecorderVideoDispose()
        {
            var imgProviderMock = _moq.GetImageProviderMock();
            var videoWriterMock = _moq.GetVideoFileWriterMock();
            var audioProviderMock = _moq.GetAudioProviderMock();

            using (new Recorder(videoWriterMock.Object, imgProviderMock.Object, 10, audioProviderMock.Object)) 
            {
            }

            imgProviderMock.Verify(imageProvider => imageProvider.Dispose(), Times.Once);
            videoWriterMock.Verify(videoFileWriter => videoFileWriter.Dispose(), Times.Once);
            audioProviderMock.Verify(audioProvider => audioProvider.Dispose(), Times.Once);
        }

        [Fact]
        public void RecorderAudioDispose()
        {
            var audioWriterMock = _moq.GetAudioFileWriterMock();
            var audioProviderMock = _moq.GetAudioProviderMock();

            using (new Recorder(audioWriterMock.Object, audioProviderMock.Object))
            {
            }
            
            audioWriterMock.Verify(audioFileWriter => audioFileWriter.Dispose(), Times.Once);
            audioProviderMock.Verify(audioProvider => audioProvider.Dispose(), Times.Once);
        }

        [Fact]
        public void StartAfterDisposed()
        {
            var imageProvider = _moq.GetImageProviderMock().Object;
            var videoWriter = _moq.GetVideoFileWriterMock().Object;

            var recorder = new Recorder(videoWriter, imageProvider, 10);

            using (recorder)
            {
            }

            Assert.Throws<ObjectDisposedException>(() => recorder.Start());
        }

        [Fact]
        public void StopAfterDisposed()
        {
            var imageProvider = _moq.GetImageProviderMock().Object;
            var videoWriter = _moq.GetVideoFileWriterMock().Object;

            var recorder = new Recorder(videoWriter, imageProvider, 10);

            using (recorder)
            {
            }

            Assert.Throws<ObjectDisposedException>(() => recorder.Stop());
        }
    }
}
