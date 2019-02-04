using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Captura.Base.Audio
{
    public abstract class AudioSource : NotifyPropertyChanged, IDisposable
    {
        protected readonly ObservableCollection<IAudioItem> RecordingSources = new ObservableCollection<IAudioItem>();
        protected readonly ObservableCollection<IAudioItem> LoopbackSources = new ObservableCollection<IAudioItem>();

        public ReadOnlyObservableCollection<IAudioItem> AvailableRecordingSources { get; }
        public ReadOnlyObservableCollection<IAudioItem> AvailableLoopbackSources { get; }

        protected AudioSource()
        {
            AvailableRecordingSources = new ReadOnlyObservableCollection<IAudioItem>(RecordingSources);
            AvailableLoopbackSources = new ReadOnlyObservableCollection<IAudioItem>(LoopbackSources);
        }

        public virtual void Dispose() { }

        public void Refresh()
        {
            // Retain previously active sources
            var lastMicNames = RecordingSources
                .Where(audioItem => audioItem.Active)
                .Select(audioItem => audioItem.Name)
                .ToArray();

            var lastSpeakerNames = LoopbackSources
                .Where(audioItem => audioItem.Active)
                .Select(audioItem => audioItem.Name)
                .ToArray();

            RecordingSources.Clear();
            LoopbackSources.Clear();

            OnRefresh();

            foreach (var source in RecordingSources)
            {
                source.Active = lastMicNames.Contains(source.Name);
            }

            foreach (var source in LoopbackSources)
            {
                source.Active = lastSpeakerNames.Contains(source.Name);
            }
        }

        protected abstract void OnRefresh();

        public abstract IAudioProvider GetMixedAudioProvider();

        public abstract IAudioProvider[] GetMultipleAudioProviders();

        public abstract string Name { get; }

        public virtual bool CanChangeSourcesDuringRecording => false;
    }
}