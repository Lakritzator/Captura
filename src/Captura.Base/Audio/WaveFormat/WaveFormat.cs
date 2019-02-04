using System;
using System.IO;

namespace Captura.Base.Audio.WaveFormat
{
    /// <summary>
    /// Represents a Wave file format
    /// </summary>
    public class WaveFormat
    {
        /// <summary>
        /// Creates a new PCM 44.1Khz stereo 16 bit format
        /// </summary>
        public WaveFormat() : this(44100, 16, 2) { }

        /// <summary>
        /// Creates a new 16 bit wave format with the specified sample
        /// rate and channel count
        /// </summary>
        /// <param name="sampleRate">Sample Rate</param>
        /// <param name="channels">Number of channels</param>
        public WaveFormat(int sampleRate, int channels) : this(sampleRate, 16, channels) { }

        /// <summary>
        /// Creates a new PCM format with the specified sample rate, bit depth and channels
        /// </summary>
        public WaveFormat(int sampleRate, int bitsPerSample, int channels)
        {
            if (channels < 1)
                throw new ArgumentOutOfRangeException(nameof(channels), "Channels must be 1 or greater");

            // minimum 16 bytes, sometimes 18 for PCM
            Encoding = WaveFormatEncoding.Pcm;
            Channels = (short)channels;
            SampleRate = sampleRate;
            BitsPerSample = (short)bitsPerSample;
            ExtraSize = 0;

            BlockAlign = (short)(channels * (bitsPerSample / 8));
            AverageBytesPerSecond = SampleRate * BlockAlign;
        }

        /// <summary>
        /// Creates a new 32 bit IEEE floating point wave format
        /// </summary>
        /// <param name="sampleRate">sample rate</param>
        /// <param name="channels">number of channels</param>
        public static WaveFormat CreateIeeeFloatWaveFormat(int sampleRate, int channels)
        {
            return new WaveFormat
            {
                Encoding = WaveFormatEncoding.Float,
                Channels = (short)channels,
                BitsPerSample = 32,
                SampleRate = sampleRate,
                BlockAlign = (short)(4 * channels),
                AverageBytesPerSecond = sampleRate * 4 * channels,
                ExtraSize = 0
            };
        }
        
        /// <summary>
        /// Returns the encoding type used
        /// </summary>
        public WaveFormatEncoding Encoding { get; set; }

        /// <summary>
        /// Writes this WaveFormat object to a stream
        /// </summary>
        /// <param name="writer">the output stream</param>
        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write((short)Encoding);
            writer.Write((short)Channels);
            writer.Write(SampleRate);
            writer.Write(AverageBytesPerSecond);
            writer.Write((short)BlockAlign);
            writer.Write((short)BitsPerSample);
            writer.Write((short)ExtraSize);
        }

        /// <summary>
        /// Returns the number of channels (1=mono,2=stereo etc)
        /// </summary>
        public int Channels { get; set; }

        /// <summary>
        /// Returns the sample rate (samples per second)
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        /// Returns the average number of bytes used per second
        /// </summary>
        public int AverageBytesPerSecond { get; set; }

        /// <summary>
        /// Returns the block alignment
        /// </summary>
        public int BlockAlign { get; set; }

        /// <summary>
        /// Returns the number of bits per sample (usually 16 or 32, sometimes 24 or 8)
        /// Can be 0 for some codecs
        /// </summary>
        public int BitsPerSample { get; set; }

        /// <summary>
        /// Returns the number of extra bytes used by this waveformat.
        /// Often 0, except for compressed formats which store extra data after the WAVEFORMATEX header
        /// </summary>
        public int ExtraSize { get; set; }
    }
}
