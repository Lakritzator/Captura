using System.Linq;
using Captura.Base.Audio;
using Captura.Base.Services;
using ManagedBass;

namespace Captura.Bass
{
    /// <summary>
    /// ManagedBass Audio Source.
    /// Use <see cref="Available"/> to check if all dependencies are present.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BassAudioSource : AudioSource
    {
        public BassAudioSource()
        {
            // Initializes Default Playback Device.
            ManagedBass.Bass.Init();

            // Enable Loopback Recording.
            ManagedBass.Bass.Configure(Configuration.LoopbackRecording, true);

            Refresh();
        }

        private static bool AllExist(params string[] paths)
        {
            return paths.All(ServiceProvider.FileExists);
        }

        // Check if all BASS dependencies are present
        public static bool Available { get; } = AllExist("ManagedBass.dll", "ManagedBass.Mix.dll", "bass.dll", "bassmix.dll");

        public override IAudioProvider GetMixedAudioProvider()
        {
            return new MixedAudioProvider(AvailableRecordingSources
                .Concat(AvailableLoopbackSources)
                .Cast<BassItem>());
        }

        public override IAudioProvider[] GetMultipleAudioProviders()
        {
            return AvailableRecordingSources.Where(audioItem => audioItem.Active)
                .Concat(AvailableLoopbackSources.Where(audioItem => audioItem.Active))
                .Cast<BassItem>()
                .Select(bassItem => new BassAudioProvider(bassItem))
                .ToArray<IAudioProvider>();
        }

        protected override void OnRefresh()
        {
            for (var i = 0; ManagedBass.Bass.RecordGetDeviceInfo(i, out var info); ++i)
            {
                if (info.IsLoopback)
                    LoopbackSources.Add(new BassItem(i, info.Name));
                else RecordingSources.Add(new BassItem(i, info.Name));
            }
        }

        /// <summary>
        /// Frees all BASS devices.
        /// </summary>
        public override void Dispose()
        {
            for (var i = 0; ManagedBass.Bass.RecordGetDeviceInfo(i, out var info); ++i)
            {
                if (info.IsInitialized)
                {
                    ManagedBass.Bass.CurrentRecordingDevice = i;
                    ManagedBass.Bass.RecordFree();
                }
            }

            for (var i = 0; ManagedBass.Bass.GetDeviceInfo(i, out var info); ++i)
            {
                if (info.IsInitialized)
                {
                    ManagedBass.Bass.CurrentDevice = i;
                    ManagedBass.Bass.Free();
                }
            }
        }

        public override string Name { get; } = "BASS";

        public override bool CanChangeSourcesDuringRecording => true;
    }
}