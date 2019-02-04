using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using Captura.Base.Services;
using Captura.FFmpeg.Settings;
using Captura.Loc;

namespace Captura.FFmpeg
{
    public static class FFmpegService
    {
        private const string FFmpegExeName = "ffmpeg.exe";

        private static FFmpegSettings GetSettings() => ServiceProvider.Get<FFmpegSettings>();

        public static bool FFmpegExists
        {
            get
            {
                var settings = GetSettings();

                // FFmpeg folder
                if (!string.IsNullOrWhiteSpace(settings.FolderPath))
                {
                    var path = Path.Combine(settings.FolderPath, FFmpegExeName);

                    if (File.Exists(path))
                        return true;
                }

                // application directory
                var cpath = Path.Combine(Assembly.GetEntryAssembly().Location, FFmpegExeName);

                if (File.Exists(cpath))
                    return true;

                // Current working directory
                if (File.Exists(FFmpegExeName))
                    return true;

                // PATH
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = FFmpegExeName,
                        Arguments = "-version",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });

                    return true;
                }
                catch { return false; }
            }
        }

        public static string FFmpegExePath
        {
            get
            {
                var settings = GetSettings();

                // FFmpeg folder
                if (!string.IsNullOrWhiteSpace(settings.FolderPath))
                {
                    var path = Path.Combine(settings.FolderPath, FFmpegExeName);

                    if (File.Exists(path))
                        return path;
                }

                // application directory
                var exePath = Path.Combine(Assembly.GetEntryAssembly().Location, FFmpegExeName);

                return File.Exists(exePath) ? exePath : FFmpegExeName;
            }
        }

        public static void SelectFFmpegFolder()
        {
            var settings = GetSettings();

            var dialogService = ServiceProvider.Get<IDialogService>();

            var folder = dialogService.PickFolder(settings.FolderPath, LanguageManager.Instance.SelectFFmpegFolder);
            
            if (!string.IsNullOrWhiteSpace(folder))
                settings.FolderPath = folder;
        }

        public static Action FFmpegDownloader { get; set; }

        public static Process StartFFmpeg(string arguments, string outputFileName)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = FFmpegExePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                },
                EnableRaisingEvents = true
            };

            var logItem = ServiceProvider.Get<FFmpegLog>().CreateNew(Path.GetFileName(outputFileName));
                        
            process.ErrorDataReceived += (sender, e) => logItem.Write(e.Data);

            process.Start();

            process.BeginErrorReadLine();
            
            return process;
        }

        public static bool WaitForConnection(this NamedPipeServerStream serverStream, int timeout)
        {
            var asyncResult = serverStream.BeginWaitForConnection(result => {}, null);

            if (asyncResult.AsyncWaitHandle.WaitOne(timeout))
            {
                serverStream.EndWaitForConnection(asyncResult);

                return serverStream.IsConnected;
            }

            return false;
        }
    }
}
