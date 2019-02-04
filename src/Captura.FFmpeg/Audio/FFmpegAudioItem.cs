using System.Collections.Generic;
using Captura.Base;
using Captura.Base.Audio;
using Captura.Base.Audio.WaveFormat;

namespace Captura.FFmpeg.Audio
{
    public class FFmpegAudioItem : NoVideoItem
    {
        public FFmpegAudioArgsProvider AudioArgsProvider { get; }

        private const string Experimental = "-strict -2";

        // The (FFmpeg) appended to the name is expected in Custom Codecs
        private FFmpegAudioItem(string name, string extension, FFmpegAudioArgsProvider audioArgsProvider)
            : base($"{name} (FFmpeg)", extension)
        {
            AudioArgsProvider = audioArgsProvider;
        }

        public override IAudioFileWriter GetAudioFileWriter(string fileName, WaveFormat wf, int audioQuality)
        {
            return new FFmpegAudioWriter(fileName, audioQuality, AudioArgsProvider, wf.SampleRate, wf.Channels);
        }

        public static FFmpegAudioArgsProvider Aac { get; } = quality =>
        {
            // bitrate: 32k to 512k (steps of 32k)
            var b = 32 * (1 + (15 * (quality - 1)) / 99);

            return $"-c:a aac {Experimental} -b:a {b}k";
        };

        public static FFmpegAudioArgsProvider Mp3 { get; } = quality =>
        {
            // quality: 9 (lowest) to 0 (highest)
            var qscale = (100 - quality) / 11;

            return $"-c:a libmp3lame -qscale:a {qscale}";
        };

        public static FFmpegAudioArgsProvider Vorbis { get; } = quality =>
        {
            // quality: 0 (lowest) to 10 (highest)
            var qscale = (10 * (quality - 1)) / 99;

            return $"-c:a libvorbis -qscale:a {qscale}";
        };

        public static FFmpegAudioArgsProvider Opus { get; } = quality =>
        {
            // quality: 0 (lowest) to 10 (highest)
            var qscale = (10 * (quality - 1)) / 99;

            return $"-c:a libopus -compression_level {qscale}";
        };

        public static IEnumerable<FFmpegAudioItem> Items { get; } = new[]
        {
            new FFmpegAudioItem("AAC", ".aac", Aac),
            new FFmpegAudioItem("Mp3", ".mp3", Mp3),
            new FFmpegAudioItem("Vorbis", ".ogg", Vorbis),
            new FFmpegAudioItem("Opus", ".opus", Opus)
        };
    }
}
