using System.ComponentModel;

namespace Captura.Base.Services
{
    public interface IWebCamItem : INotifyPropertyChanged
    {
        string Name { get; }
    }
}