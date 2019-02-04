using System;
using Captura.Base.Audio;
using Captura.Base.Images;
using Captura.Base.Video;
using SharpAvi.Codecs;
using SharpAvi.Output;
using AviInternalWriter = SharpAvi.Output.AviWriter;

namespace Captura.SharpAvi
{
    /// <summary>
    /// Writes an AVI file.
    /// </summary>
    internal class AviWriter : IVideoFileWriter
    {
        #region Fields

        private AviInternalWriter _writer;
        private IAviVideoStream _videoStream;
        private IAviAudioStream _audioStream;
        private byte[] _videoBuffer;
        private readonly AviCodec _codec;
        private readonly object _syncLock = new object();
        
        /// <summary>
        /// Gets whether Audio is recorded.
        /// </summary>
        public bool SupportsAudio => _audioStream != null;
        #endregion

        /// <summary>
        /// Creates a new instance of <see cref="AviWriter"/>.
        /// </summary>
        /// <param name="fileName">Output file path.</param>
        /// <param name="codec">The Avi Codec.</param>
        /// <param name="imageProvider">The image source.</param>
        /// <param name="frameRate">Video Frame Rate.</param>
        /// <param name="audioProvider">The audio source. null = no audio.</param>
        public AviWriter(string fileName, AviCodec codec, IImageProvider imageProvider, int frameRate, IAudioProvider audioProvider = null)
        {
            _codec = codec;

            _videoBuffer = new byte[imageProvider.Width * imageProvider.Height * 4];

            _writer = new AviInternalWriter(fileName)
            {
                FramesPerSecond = frameRate,
                EmitIndex1 = true
            };

            CreateVideoStream(imageProvider.Width, imageProvider.Height);

            if (audioProvider != null)
                CreateAudioStream(audioProvider);
        }
        
        /// <summary>
        /// Writes an Image frame.
        /// </summary>
        public void WriteFrame(IBitmapFrame frame)
        {
            if (!(frame is RepeatFrame))
            {
                using (frame)
                {
                    frame.CopyTo(_videoBuffer, _videoBuffer.Length);
                }
            }

            lock (_syncLock)
                _videoStream.WriteFrame(true, _videoBuffer, 0, _videoBuffer.Length);
        }

        private void CreateVideoStream(int width, int height)
        {
            // Select encoder type based on FOURCC of codec
            if (_codec == AviCodec.Uncompressed)
                _videoStream = _writer.AddUncompressedVideoStream(width, height);
            else if (_codec == AviCodec.MotionJpeg)
            {
                // MotionJpegVideoStream implementation allocates multiple WriteableBitmap for every thread
                // Use SingleThreadWrapper to reduce allocation
                var encoderFactory = new Func<IVideoEncoder>(() => new MotionJpegVideoEncoderWpf(width, height, _codec.Quality));
                var encoder = new SingleThreadedVideoEncoderWrapper(encoderFactory);

                _videoStream = _writer.AddEncodingVideoStream(encoder, true, width, height);
            }
            else
            {
                _videoStream = _writer.AddMpeg4VideoStream(width, height,
                    (double)_writer.FramesPerSecond,
                    // It seems that all tested MPEG-4 VfW codecs ignore the quality affecting parameters passed through VfW API
                    // They only respect the settings from their own configuration dialogs, and Mpeg4VideoEncoder currently has no support for this
                    0,
                    _codec.Quality,
                    // Most of VfW codecs expect single-threaded use, so we wrap this encoder to special wrapper
                    // Thus all calls to the encoder (including its instantiation) will be invoked on a single thread although encoding (and writing) is performed asynchronously
                    _codec.FourCC,
                    true);
            }

            _videoStream.Name = "Video";
        }

        private void CreateAudioStream(IAudioProvider audioProvider)
        {
            _audioStream = _writer.AddEncodingAudioStream(new AudioProviderAdapter(audioProvider));

            _audioStream.Name = "Audio";
        }

        /// <summary>
        /// Write audio block to Audio Stream.
        /// </summary>
        /// <param name="buffer">Buffer containing audio data.</param>
        /// <param name="length">Length of audio data in bytes.</param>
        public void WriteAudio(byte[] buffer, int length)
        {
            lock (_syncLock)
                _audioStream?.WriteBlock(buffer, 0, length);
        }

        /// <summary>
        /// Frees all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            lock (_syncLock)
            {
                _writer.Close();
                _writer = null;

                _videoStream = null;
                _audioStream = null;
            }

            _videoBuffer = null;
        }
    }
}
