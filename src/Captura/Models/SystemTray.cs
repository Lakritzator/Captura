using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows.Controls.Primitives;
using Captura.Base.Audio;
using Captura.Base.Notification;
using Captura.Base.Services;
using Captura.Controls;
using Captura.Core.Settings;

namespace Captura.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SystemTray : ISystemTray
    {
        private bool _first = true;

        /// <summary>
        /// Using a Function ensures that the <see cref="TaskbarIcon"/> object is used only after it is initialised.
        /// </summary>
        private readonly Func<TaskbarIcon> _trayIcon;

        private readonly Settings _settings;
        private readonly IAudioPlayer _audioPlayer;

        private readonly NotificationStack _notificationStack = new NotificationStack();

        public SystemTray(Func<TaskbarIcon> taskbarIcon, Settings settings, IAudioPlayer audioPlayer)
        {
            _trayIcon = taskbarIcon;
            _settings = settings;
            _audioPlayer = audioPlayer;

            _notificationStack.Opacity = 0;
        }

        public void HideNotification()
        {
            _notificationStack.Hide();
        }

        private void Show()
        {
            var trayIcon = _trayIcon.Invoke();

            if (trayIcon != null && _first)
            {
                trayIcon.ShowCustomBalloon(_notificationStack, PopupAnimation.None, null);

                _first = false;
            }

            _audioPlayer.Play(SoundKind.Notification);

            _notificationStack.Show();
        }

        public void ShowScreenShotNotification(string filePath)
        {
            if (!_settings.Tray.ShowNotifications)
                return;

            _notificationStack.Add(new ScreenShotBalloon(filePath));

            Show();
        }

        public void ShowNotification(INotification notification)
        {
            if (!_settings.Tray.ShowNotifications)
                return;

            _notificationStack.Add(new NotificationBalloon(notification));

            Show();
        }
    }
}
