using System;
using System.IO;
using System.Text;
using Captura.Base.Audio;
using Captura.Base.Audio.WaveFormat;

namespace Screna
{
    /// <summary>
    /// Writes an Audio file.
    /// </summary>
    public class AudioFileWriter : IAudioFileWriter
    {
        private readonly object _syncLock = new object();
        private readonly BinaryWriter _writer;
        private readonly WaveFormat _format;

        private readonly long _dataSizePos, _factSampleCountPos;

        private readonly bool _riff;

        /// <summary>
        /// Creates a new instance of <see cref="AudioFileWriter"/>.
        /// </summary>
        public AudioFileWriter(Stream outStream, WaveFormat format, bool riff = true)
        {
            if (outStream == null)
            {
                throw new ArgumentNullException(nameof(outStream));
            }

            _format = format ?? throw new ArgumentNullException(nameof(format));
            _riff = riff;

            _writer = new BinaryWriter(outStream, Encoding.UTF8);

            if (riff)
            {
                _writer.Write(Encoding.UTF8.GetBytes("RIFF"));
                _writer.Write(0); // placeholder
                _writer.Write(Encoding.UTF8.GetBytes("WAVE"));

                _writer.Write(Encoding.UTF8.GetBytes("fmt "));

                _writer.Write(18 + format.ExtraSize); // wave format length
            }

            format.Serialize(_writer);

            if (!riff)
            {
                return;
            }

            // CreateFactChunk
            if (HasFactChunk)
            {
                _writer.Write(Encoding.UTF8.GetBytes("fact"));
                _writer.Write(4);
                _factSampleCountPos = outStream.Position;
                _writer.Write(0); // number of samples
            }

            // WriteDataChunkHeader
            _writer.Write(Encoding.UTF8.GetBytes("data"));
            _dataSizePos = outStream.Position;
            _writer.Write(0); // placeholder
        }

        /// <summary>
        /// Creates a new instance of <see cref="AudioFileWriter"/>.
        /// </summary>
        public AudioFileWriter(string fileName, WaveFormat format, bool riff = true)
            : this(new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read), format, riff) { }

        private bool HasFactChunk => _format.Encoding != WaveFormatEncoding.Pcm && _format.BitsPerSample != 0;
        
        /// <summary>
        /// Number of bytes of audio in the data chunk
        /// </summary>
        public long Length { get; private set; }

        /// <summary>
        /// Writes to file.
        /// </summary>
        public void Write(byte[] data, int offset, int count)
        {
            lock (_syncLock)
            {
                if (_riff && _writer.BaseStream.Length + count > uint.MaxValue)
                {
                    throw new ArgumentException("WAV file too large", nameof(count));
                }
                
                _writer.Write(data, offset, count);
                Length += count;
            }
        }

        /// <summary>
        /// Writes all buffered data to file.
        /// </summary>
        public void Flush()
        {
            lock (_syncLock)
            {
                _writer.Flush();

                if (!_riff)
                {
                    return;
                }

                var pos = _writer.BaseStream.Position;
                UpdateHeader();
                _writer.BaseStream.Position = pos;
            }
        }
        
        /// <summary>
        /// Updates the header with file size information
        /// </summary>
        private void UpdateHeader()
        {
            // UpdateRiffChunk
            _writer.Seek(4, SeekOrigin.Begin);
            _writer.Write((uint)(_writer.BaseStream.Length - 8));

            // UpdateFactChunk
            if (HasFactChunk)
            {
                var bitsPerSample = _format.BitsPerSample * _format.Channels;
                if (bitsPerSample != 0)
                {
                    _writer.Seek((int)_factSampleCountPos, SeekOrigin.Begin);

                    _writer.Write((int)(Length * 8 / bitsPerSample));
                }
            }

            // UpdateDataChunk
            _writer.Seek((int)_dataSizePos, SeekOrigin.Begin);
            _writer.Write((uint)Length);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            using (_writer)
            {
                Flush();
            }
        }
    }
}