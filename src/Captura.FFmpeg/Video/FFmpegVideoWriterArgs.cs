using Captura.Base.Video;
using Captura.FFmpeg.Audio;

namespace Captura.FFmpeg.Video
{
    public class FFmpegVideoWriterArgs : VideoWriterArgs
    {
        public static FFmpegVideoWriterArgs FromVideoWriterArgs(VideoWriterArgs args, FFmpegVideoArgsProvider videoArgsProvider, FFmpegAudioArgsProvider audioArgsProvider)
        {
            return new FFmpegVideoWriterArgs
            {
                FileName = args.FileName,
                ImageProvider = args.ImageProvider,
                FrameRate = args.FrameRate,
                VideoQuality = args.VideoQuality,
                VideoArgsProvider = videoArgsProvider,
                AudioQuality = args.AudioQuality,
                AudioArgsProvider = audioArgsProvider,
                AudioProvider = args.AudioProvider
            };
        }

        public FFmpegVideoArgsProvider VideoArgsProvider { get; set; }
        public FFmpegAudioArgsProvider AudioArgsProvider { get; set; }
        public int Frequency { get; set; } = 44100;
        public int Channels { get; set; } = 2;
        public string OutputArgs { get; set; } = "";
    }
}