using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Captura.Base
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            RaisePropertyChanged(propertyName);
        }

        protected void RaiseAllChanged()
        {
            RaisePropertyChanged("");
        }
    }
}
