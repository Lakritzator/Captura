using CommandLine;

namespace Captura.CmdOptions
{
    [Verb("ffmpeg", HelpText = "Manage FFmpeg")]
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class FFmpegCmdOptions
    {
        [Option("install", HelpText = "Install FFmpeg to specified folder.")]
        public string Install { get; set; }
    }
}
