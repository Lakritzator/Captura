using System;
using System.Windows;
using System.Windows.Input;
using Captura.Base.Notification;
using Captura.Models;

namespace Captura.Controls
{
    public partial class NotificationBalloon : IRemoveRequester
    {
        public INotification Notification { get; }

        public NotificationBalloon(INotification notification)
        {
            Notification = notification;

            notification.RemoveRequested += OnClose;

            InitializeComponent();
        }

        private void OnClose()
        {
            RemoveRequested?.Invoke();
        }

        public event Action RemoveRequested;

        private void CloseButton_Click(object sender, RoutedEventArgs e) => OnClose();

        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Notification.RaiseClick();

            OnClose();
        }
    }
}