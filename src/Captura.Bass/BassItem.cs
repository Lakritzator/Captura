using Captura.Base;
using Captura.Base.Audio;

namespace Captura.Bass
{
    class BassItem : NotifyPropertyChanged, IAudioItem
    {
        public int Id { get; }

        public BassItem(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name { get; }

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