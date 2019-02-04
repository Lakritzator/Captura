using System;

namespace Captura.Base.Services
{
    public interface IMessageProvider
    {
        void ShowError(string message, string header = null);

        bool ShowYesNo(string message, string title);

        void ShowFFmpegUnavailable();

        void ShowException(Exception exception, string message, bool blocking = false);
    }
}
