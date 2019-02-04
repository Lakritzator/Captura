using System.ComponentModel;

namespace Captura.Base.Audio
{
    public interface IAudioItem : INotifyPropertyChanged
    {
        string Name { get; }

        bool Active { get; set; }
    }
}
