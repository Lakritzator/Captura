namespace Captura.FFmpeg.ArgsBuilder
{
    public class FFmpegInputArgs : FFmpegArgs
    {
        private readonly string _input;

        public FFmpegInputArgs(string input)
        {
            _input = input;
        }

        public override string GetArgs()
        {
            return base.GetArgs() + $" -i {_input}";
        }

        public FFmpegInputArgs AddArg(string arg)
        {
            Args.Add(arg);

            return this;
        }

        public FFmpegInputArgs SetVideoSize(int width, int height)
        {
            return AddArg($"-video_size {width}x{height}");
        }

        public FFmpegInputArgs SetFrameRate(int frameRate)
        {
            return AddArg($"-r {frameRate}");
        }

        public FFmpegInputArgs SetFormat(string format)
        {
            return AddArg($"-f {format}");
        }

        public FFmpegInputArgs SetAudioCodec(string codec)
        {
            return AddArg($"-acodec {codec}");
        }

        public FFmpegInputArgs SetAudioFrequency(int frequency)
        {
            return AddArg($"-ar {frequency}");
        }

        public FFmpegInputArgs SetAudioChannels(int channels)
        {
            return AddArg($"-ac {channels}");
        }

        public FFmpegInputArgs DisableVideo()
        {
            return AddArg("-vn");
        }
    }
}