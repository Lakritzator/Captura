namespace Captura.FFmpeg.ArgsBuilder
{
    public class FFmpegOutputArgs : FFmpegArgs
    {
        private readonly string _output;

        public FFmpegOutputArgs(string output)
        {
            _output = output;
        }

        public override string GetArgs()
        {
            return base.GetArgs() + $" {_output}";
        }

        public FFmpegOutputArgs AddArg(string arg)
        {
            Args.Add(arg);

            return this;
        }

        public FFmpegOutputArgs SetVideoSize(int width, int height)
        {
            return AddArg($"-video_size {width}x{height}");
        }

        public FFmpegOutputArgs SetFrameRate(int frameRate)
        {
            return AddArg($"-r {frameRate}");
        }
    }
}