using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Captura.Base.Services;
using Captura.CmdOptions;
using Captura.Core;
using Captura.Fakes;
using Captura.FFmpeg;
using Captura.Native;
using CommandLine;
using static System.Console;
// ReSharper disable LocalizableElement

namespace Captura
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(location))
                {
                    return;
                }
                var uiPath = Path.Combine(location, "captura.exe");

                if (File.Exists(uiPath))
                {
                    Process.Start(uiPath);

                    return;
                }
            }

            User32.SetProcessDPIAware();

            ServiceProvider.LoadModule(new CoreModule());
            ServiceProvider.LoadModule(new FakesModule());

            Parser.Default.ParseArguments<StartCmdOptions, ShotCmdOptions, FFmpegCmdOptions, ListCmdOptions>(args)
                .WithParsed((ListCmdOptions options) =>
                {
                    Banner();

                    var lister = ServiceProvider.Get<ConsoleLister>();

                    lister.List();
                })
                .WithParsed((StartCmdOptions options) =>
                {
                    Banner();

                    using (var manager = ServiceProvider.Get<ConsoleManager>())
                    {
                        manager.CopySettings();

                        manager.Start(options);
                    }
                })
                .WithParsed((ShotCmdOptions options) =>
                {
                    Banner();

                    using (var manager = ServiceProvider.Get<ConsoleManager>())
                    {
                        manager.Shot(options);
                    }
                })
                .WithParsed((FFmpegCmdOptions options) =>
                {
                    Banner();

                    FFmpeg(options);
                });
        }

        private static void Banner()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

            WriteLine($@"Captura v{version}
(c) {DateTime.Now.Year} Mathew Sachin
");
        }

        private static async void FFmpeg(FFmpegCmdOptions fFmpegOptions)
        {
            if (fFmpegOptions.Install != null)
            {
                var downloadFolder = fFmpegOptions.Install;

                if (!Directory.Exists(downloadFolder))
                {
                    WriteLine("Directory doesn't exist");
                    return;
                }

                var ffMpegDownload = ServiceProvider.Get<FFmpegDownloadViewModel>();

                ffMpegDownload.TargetFolder = fFmpegOptions.Install;

                await ffMpegDownload.Start();
                
                WriteLine(ffMpegDownload.Status);
            }
        }
    }
}
