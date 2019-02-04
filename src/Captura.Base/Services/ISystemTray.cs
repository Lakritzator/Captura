using Captura.Base.Notification;

namespace Captura.Base.Services
{
    public interface ISystemTray
    {
        void ShowScreenShotNotification(string filePath);

        void HideNotification();

        void ShowNotification(INotification notification);
    }
}
