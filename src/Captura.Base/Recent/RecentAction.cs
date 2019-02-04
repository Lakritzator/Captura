using System;
using System.Windows.Input;

namespace Captura.Base.Recent
{
    public class RecentAction
    {
        public RecentAction(string name, string icon, Action onClick)
        {
            Name = name;
            Icon = icon;

            ClickCommand = new DelegateCommand(() => onClick?.Invoke());
        }

        public string Name { get; set; }

        public string Icon { get; }

        public ICommand ClickCommand { get; }
    }
}