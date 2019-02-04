using System;
using System.Collections.Generic;

namespace Captura.Base.Notification
{
    public interface INotification
    {
        int Progress { get; }

        string PrimaryText { get; }

        string SecondaryText { get; }

        bool Finished { get; }

        IEnumerable<NotificationAction> Actions { get; }

        void RaiseClick();

        void Remove();

        event Action RemoveRequested;
    }
}