using Captura.Base;
using Captura.Base.Audio;
using NAudio.CoreAudioApi;

namespace Captura.NAudio
{
    class NAudioItem : NotifyPropertyChanged, IAudioItem
    {
        public MMDevice Device { get; }

        public string Name { get; }

        public NAudioItem(MMDevice device)
        {
            Device = device;

            Name = device.FriendlyName;
        }

        bool _active;

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;

                OnPropertyChanged();
            }
        }

        public override string ToString() => Name;
    }
}