using System.Collections.Generic;
using Captura.Base.Video;
using Captura.FFmpeg.Audio;

namespace Captura.FFmpeg.Video
{
    // ReSharper disable once InconsistentNaming
    public class FFmpegPostProcessingItem : IVideoWriterItem
    {
        private readonly string _name;
        private readonly FFmpegVideoArgsProvider _videoArgsProvider;
        private readonly FFmpegAudioArgsProvider _audioArgsProvider;

        public FFmpegPostProcessingItem(string Name, string extension, FFmpegVideoArgsProvider videoArgsProvider, FFmpegAudioArgsProvider audioArgsProvider)
        {
            _name = Name;
            _videoArgsProvider = videoArgsProvider;
            _audioArgsProvider = audioArgsProvider;
            Extension = extension;
        }

        public string Extension { get; }

        public IVideoFileWriter GetVideoFileWriter(VideoWriterArgs args)
        {
            return new FFmpegPostProcessingWriter(FFmpegVideoWriterArgs.FromVideoWriterArgs(args, _videoArgsProvider, _audioArgsProvider));
        }

        public override string ToString() => $"Post Processing: {_name}";

        public string Description => "Encoding is done after recording has been finished.";

        public static IEnumerable<FFmpegPostProcessingItem> Items { get; } = new[]
        {
            new FFmpegPostProcessingItem("WebM (Vp8, Opus)", ".webm", videoQuality =>
            {
                // quality: 63 (lowest) to 4 (highest)
                var crf = 63 - ((videoQuality - 1) * 59) / 99;

                return $"-vcodec libvpx -crf {crf} -b:v 1M";
            }, FFmpegAudioItem.Opus),

            new FFmpegPostProcessingItem("WebM (Vp9, Opus)", ".webm", videoQuality =>
            {
                // quality: 63 (lowest) to 0 (highest)
                var crf = (63 * (100 - videoQuality)) / 99;

                return $"-vcodec libvpx-vp9 -crf {crf} -b:v 0";
            }, FFmpegAudioItem.Opus)
        };
    }
}