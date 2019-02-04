using System.IO;
using Captura.Base.Images;
using Captura.Base.Video;
using Captura.FFmpeg.ArgsBuilder;

namespace Captura.FFmpeg.Video
{
    // ReSharper disable once InconsistentNaming
    public class FFmpegGifWriter : IVideoFileWriter
    {
        private readonly IVideoFileWriter _ffMpegWriter;
        private readonly string _tempFileName;
        private readonly VideoWriterArgs _args;

        public FFmpegGifWriter(VideoWriterArgs Args)
        {
            _args = Args;
            _tempFileName = Path.GetTempFileName();

            _ffMpegWriter = FFmpegItem.x264.GetVideoFileWriter(new VideoWriterArgs
            {
                FileName = _tempFileName,
                FrameRate = Args.FrameRate,
                ImageProvider = Args.ImageProvider,
                VideoQuality = Args.VideoQuality
            }, "-f mp4 -y");
        }

        private string GeneratePalette()
        {
            var argsBuilder = new FFmpegArgsBuilder();

            argsBuilder.AddInputFile(_tempFileName);

            var tempFile = Path.GetTempFileName();
            var paletteFile = Path.ChangeExtension(tempFile, "png");
            File.Move(tempFile, paletteFile);

            argsBuilder.AddOutputFile(paletteFile)
                .AddArg("-vf palettegen")
                .SetFrameRate(_args.FrameRate)
                .AddArg("-y");

            var process = FFmpegService.StartFFmpeg(argsBuilder.GetArgs(), paletteFile);

            process.WaitForExit();

            return paletteFile;
        }

        private void GenerateGif(string paletteFile)
        {
            var argsBuilder = new FFmpegArgsBuilder();

            argsBuilder.AddInputFile(_tempFileName);

            argsBuilder.AddInputFile(paletteFile);

            argsBuilder.AddOutputFile(_args.FileName)
                .AddArg("-lavfi paletteuse")
                .SetFrameRate(_args.FrameRate);

            var process = FFmpegService.StartFFmpeg(argsBuilder.GetArgs(), _args.FileName);

            process.WaitForExit();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _ffMpegWriter.Dispose();

            var palletteFile = GeneratePalette();

            GenerateGif(palletteFile);

            File.Delete(palletteFile);
            File.Delete(_tempFileName);
        }

        /// <inheritdoc />
        public bool SupportsAudio { get; } = true;
        
        /// <inheritdoc />
        public void WriteAudio(byte[] buffer, int length)
        {
            _ffMpegWriter.WriteAudio(buffer, length);
        }

        /// <inheritdoc />
        public void WriteFrame(IBitmapFrame frame)
        {
            _ffMpegWriter.WriteFrame(frame);
        }
    }
}
