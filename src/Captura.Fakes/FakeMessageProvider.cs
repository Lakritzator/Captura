using System;
using Captura.Base.Services;

namespace Captura.Fakes
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class FakeMessageProvider : IMessageProvider
    {
        public void ShowError(string message, string header = null)
        {
            if (header != null)
                Console.WriteLine(header);

            Console.Error.WriteLine(message);
        }

        public void ShowFFmpegUnavailable()
        {
            ShowError("FFmpeg is not available.\nYou can install ffmpeg by calling: captura ffmpeg --install [path]");
        }

        public void ShowException(Exception exception, string message, bool blocking = false)
        {
            ShowError(exception.ToString());
        }

        public bool ShowYesNo(string message, string title)
        {
            Console.Write($"{message} (Y/N): ");

            var reply = Console.ReadLine();

            return reply != null && reply.ToLower() == "y";
        }
    }
}
