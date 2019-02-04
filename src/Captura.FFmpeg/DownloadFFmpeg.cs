using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Captura.FFmpeg
{
    public static class DownloadFFmpeg
    {
        private static readonly Uri FFmpegUri;
        private static readonly string FFmpegArchivePath;

        static DownloadFFmpeg()
        {
            var bits = Environment.Is64BitOperatingSystem ? 64 : 32;

            FFmpegUri = new Uri($"https://ffmpeg.zeranoe.com/builds/win{bits}/static/ffmpeg-latest-win{bits}-static.zip");

            FFmpegArchivePath = Path.Combine(Path.GetTempPath(), "ffmpeg.zip");
        }

        public static async Task DownloadArchive(Action<int> progress, IWebProxy proxy, CancellationToken cancellationToken)
        {
            using (var webClient = new WebClient { Proxy = proxy })
            {
                cancellationToken.Register(() => webClient.CancelAsync());

                webClient.DownloadProgressChanged += (sender, e) =>
                {
                    progress?.Invoke(e.ProgressPercentage);
                };
                
                await webClient.DownloadFileTaskAsync(FFmpegUri, FFmpegArchivePath);
            }
        }

        private const string ExeName = "ffmpeg.exe";

        public static async Task ExtractTo(string folderPath)
        {
            await Task.Run(() =>
            {
                using (var archive = ZipFile.OpenRead(FFmpegArchivePath))
                {
                    var ffmpegEntry = archive.Entries.First(zipArchiveEntry => zipArchiveEntry.Name == ExeName);

                    ffmpegEntry.ExtractToFile(Path.Combine(folderPath, ExeName), true);
                }
            });
        }
    }
}