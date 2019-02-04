using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Captura.Base.Recent
{
    public interface IRecentItem
    {
        string Display { get; }

        string Icon { get; }

        string IconColor { get; }

        bool IsSaving { get; }

        event Action RemoveRequested;

        ICommand ClickCommand { get; }
        ICommand RemoveCommand { get; }

        IEnumerable<RecentAction> Actions { get; }
    }
}