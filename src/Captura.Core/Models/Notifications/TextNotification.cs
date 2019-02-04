using System;
using System.Collections.Generic;
using Captura.Base.Notification;

namespace Captura.Core.Models.Notifications
{
    public class TextNotification : INotification
    {
        readonly Action _onClick;

        public TextNotification(string primaryText, Action onClick = null, string secondaryText = null)
        {
            _onClick = onClick;

            PrimaryText = primaryText;
            SecondaryText = secondaryText;
        }

        public int Progress => 0;

        public string PrimaryText { get; }
        public string SecondaryText { get; }

        bool INotification.Finished => true;

        public IEnumerable<NotificationAction> Actions { get; } = new NotificationAction[0];

        public void RaiseClick() => _onClick?.Invoke();

        public void Remove() => RemoveRequested?.Invoke();

        public event Action RemoveRequested;
    }
}