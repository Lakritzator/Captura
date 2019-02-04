using System;
using Captura.Base;
using Captura.Base.Video;
using Captura.Loc;

namespace Screna.VideoSourceProviders
{
    public abstract class VideoSourceProviderBase : NotifyPropertyChanged, IVideoSourceProvider
    {
        protected readonly LanguageManager Loc;

        protected VideoSourceProviderBase(LanguageManager loc)
        {
            Loc = loc;

            loc.LanguageChanged += cultureInfo => RaisePropertyChanged(nameof(Name));
        }

        public abstract IVideoItem Source { get; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract string Icon { get; }

        public virtual bool OnSelect() => true;

        public virtual void OnUnselect() { }

        public event Action UnselectRequested;

        protected void RequestUnselect()
        {
            UnselectRequested?.Invoke();
        }

        public virtual string Serialize()
        {
            return Source.ToString();
        }

        public abstract bool Deserialize(string serialized);

        public abstract bool ParseCli(string arg);
    }
}