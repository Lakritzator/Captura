using Captura.Base.Audio;
using Captura.Base.Services;
using Captura.Bass;
using Captura.FFmpeg;
using Captura.FFmpeg.Video;
using Captura.SharpAvi;
using static System.Console;

namespace Captura
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ConsoleLister
    {
        private static readonly string Underline = $"\n{new string('-', 30)}";

        private readonly IWebCamProvider _webCam;
        private readonly AudioSource _audioSource;
        private readonly IPlatformServices _platformServices;

        public ConsoleLister(IWebCamProvider webCam,
            AudioSource audioSource,
            IPlatformServices platformServices)
        {
            _webCam = webCam;
            _audioSource = audioSource;
            _platformServices = platformServices;
        }

        public void List()
        {
            FFmpeg();

            SharpAvi();

            Windows();

            Screens();

            Audio();

            WebCam();
        }

        private void WebCam()
        {
            if (_webCam.AvailableCams.Count <= 1)
            {
                return;
            }

            WriteLine("AVAILABLE WEBCAMS" + Underline);

            for (var i = 1; i < _webCam.AvailableCams.Count; ++i)
            {
                WriteLine($"{(i - 1).ToString().PadRight(2)}: {_webCam.AvailableCams[i]}");
            }

            WriteLine();
        }

        private void Audio()
        {
            WriteLine($"ManagedBass Available: {(_audioSource is BassAudioSource ? "YES" : "NO")}");

            WriteLine();

            #region Microphones

            if (_audioSource.AvailableRecordingSources.Count > 0)
            {
                WriteLine("AVAILABLE MICROPHONES" + Underline);

                for (var i = 0; i < _audioSource.AvailableRecordingSources.Count; ++i)
                {
                    WriteLine($"{i.ToString().PadRight(2)}: {_audioSource.AvailableRecordingSources[i]}");
                }

                WriteLine();
            }

            #endregion

            #region Speaker

            if (_audioSource.AvailableLoopbackSources.Count > 0)
            {
                WriteLine("AVAILABLE SPEAKER SOURCES" + Underline);

                for (var i = 0; i < _audioSource.AvailableLoopbackSources.Count; ++i)
                {
                    WriteLine($"{i.ToString().PadRight(2)}: {_audioSource.AvailableLoopbackSources[i]}");
                }

                WriteLine();
            }

            #endregion
        }

        private void Screens()
        {
            WriteLine("AVAILABLE SCREENS" + Underline);

            var j = 0;

            // First is Full Screen, Second is Screen Picker
            foreach (var screen in _platformServices.EnumerateScreens())
            {
                WriteLine($"{j.ToString().PadRight(2)}: {screen.DeviceName}");

                ++j;
            }

            WriteLine();
        }

        private void Windows()
        {
            WriteLine("AVAILABLE WINDOWS" + Underline);

            // Window Picker is skipped automatically
            foreach (var source in _platformServices.EnumerateWindows())
            {
                WriteLine($"{source.Handle.ToString().PadRight(10)}: {source.Title}");
            }

            WriteLine();
        }

        private static void SharpAvi()
        {
            var sharpAviExists = ServiceProvider.FileExists("SharpAvi.dll");

            WriteLine($"SharpAvi Available: {(sharpAviExists ? "YES" : "NO")}");

            WriteLine();

            if (sharpAviExists)
            {
                WriteLine("SharpAvi ENCODERS" + Underline);

                var writerProvider = ServiceProvider.Get<SharpAviWriterProvider>();

                var i = 0;

                foreach (var codec in writerProvider)
                {
                    WriteLine($"{i.ToString().PadRight(2)}: {codec}");
                    ++i;
                }

                WriteLine();
            }
        }

        private static void FFmpeg()
        {
            var ffmpegExists = FFmpegService.FFmpegExists;

            WriteLine($"FFmpeg Available: {(ffmpegExists ? "YES" : "NO")}");

            WriteLine();

            if (!ffmpegExists)
            {
                return;
            }

            WriteLine("FFmpeg ENCODERS" + Underline);

            var writerProvider = ServiceProvider.Get<FFmpegWriterProvider>();

            var i = 0;

            foreach (var codec in writerProvider)
            {
                WriteLine($"{i.ToString().PadRight(2)}: {codec}");
                ++i;
            }

            WriteLine();
        }
    }
}