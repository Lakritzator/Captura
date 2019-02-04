using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Captura.Base.Audio;
using ManagedBass;
using ManagedBass.Mix;
using Wf = Captura.Base.Audio.WaveFormat.WaveFormat;

namespace Captura.Bass
{
    /// <summary>
    /// Provides mixed audio from Microphone input and Speaker Output (Wasapi Loopback).
    /// Requires the presence of bass.dll and bassmix.dll.
    /// </summary>
    internal class MixedAudioProvider : IAudioProvider
    {
        private class RecordingItem
        {
            public int DeviceId { get; set; }

            public int RecordingHandle { get; set; }

            public int SilenceHandle { get; set; }
        }

        private readonly Dictionary<int, RecordingItem> _devices = new Dictionary<int, RecordingItem>();
        private readonly int _mixer;
        private readonly object _syncLock = new object();
        private bool _running;

        /// <summary>
        /// Creates a new instance of <see cref="MixedAudioProvider"/>.
        /// </summary>
        public MixedAudioProvider(IEnumerable<BassItem> devices)
        {
            if (devices == null)
                throw new ArgumentNullException();

            for (var i = 0; i < BufferCount; ++i)
            {
                _buffers.Add(new byte[0]);
            }

            _mixer = BassMix.CreateMixerStream(44100, 2, BassFlags.MixerNonStop);

            foreach (var recordingDevice in devices)
            {
                InitDevice(recordingDevice);
            }

            // mute the mixer
            ManagedBass.Bass.ChannelSetAttribute(_mixer, ChannelAttribute.Volume, 0);

            ManagedBass.Bass.ChannelSetDSP(_mixer, Procedure);
        }

        private void InitDevice(BassItem device)
        {
            _devices.Add(device.Id, new RecordingItem
            {
                DeviceId = device.Id
            });

            device.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != nameof(device.Active))
                {
                    return;
                }

                if (device.Active)
                {
                    AddDevice(device);
                }
                else
                {
                    RemoveDevice(device);
                }
            };

            if (device.Active)
                AddDevice(device);
        }

        private void RemoveDevice(BassItem device)
        {
            lock (_syncLock)
            {
                if (_devices[device.Id].RecordingHandle == 0)
                    return;

                var handle = _devices[device.Id].RecordingHandle;

                BassMix.MixerRemoveChannel(handle);

                ManagedBass.Bass.StreamFree(handle);

                _devices[device.Id].RecordingHandle = 0;

                ManagedBass.Bass.StreamFree(_devices[device.Id].SilenceHandle);

                _devices[device.Id].SilenceHandle = 0;
            }
        }

        private static int FindPlaybackDevice(DeviceInfo loopbackDeviceInfo)
        {
            for (var i = 0; ManagedBass.Bass.GetDeviceInfo(i, out var info); ++i)
            {
                if (info.Driver == loopbackDeviceInfo.Driver)
                    return i;
            }

            return -1;
        }

        private void AddDevice(BassItem device)
        {
            lock (_syncLock)
            {
                if (_devices[device.Id].RecordingHandle != 0)
                    return;

                ManagedBass.Bass.RecordInit(device.Id);

                var devInfo = ManagedBass.Bass.RecordGetDeviceInfo(device.Id);

                if (devInfo.IsLoopback)
                {
                    var playbackDevice = FindPlaybackDevice(devInfo);

                    if (playbackDevice != -1)
                    {
                        ManagedBass.Bass.Init(playbackDevice);
                        ManagedBass.Bass.CurrentDevice = playbackDevice;

                        var silence = ManagedBass.Bass.CreateStream(44100, 2, BassFlags.Default, Extensions.SilenceStreamProcedure);

                        ManagedBass.Bass.ChannelSetAttribute(silence, ChannelAttribute.Volume, 0);

                        _devices[device.Id].SilenceHandle = silence;
                    }
                }

                ManagedBass.Bass.CurrentRecordingDevice = device.Id;

                var info = ManagedBass.Bass.RecordingInfo;

                var handle = ManagedBass.Bass.RecordStart(info.Frequency, info.Channels, BassFlags.RecordPause, null);

                _devices[device.Id].RecordingHandle = handle;

                BassMix.MixerAddChannel(_mixer, handle, BassFlags.MixerDownMix | BassFlags.MixerLimit);

                if (_running)
                {
                    ManagedBass.Bass.ChannelPlay(handle);
                }
            }
        }

        private const int BufferCount = 3;

        private int _bufferIndex;

        private readonly List<byte[]> _buffers = new List<byte[]>();

        private byte[] GetBuffer(int length)
        {
            _bufferIndex = ++_bufferIndex % BufferCount;

            if (_buffers[_bufferIndex] == null || _buffers[_bufferIndex].Length < length)
            {
                _buffers[_bufferIndex] = new byte[length + 1000];

                Console.WriteLine($"New Audio Buffer Allocated: {length}");
            }

            return _buffers[_bufferIndex];
        }

        private void Procedure(int handle, int channel, IntPtr buffer, int length, IntPtr user)
        {
            var newBuffer = GetBuffer(length);

            Marshal.Copy(buffer, newBuffer, 0, length);

            Task.Run(() => DataAvailable?.Invoke(this, new DataAvailableEventArgs(newBuffer, length)));
        }
        
        /// <summary>
        /// Gets the WaveFormat of this <see cref="IAudioProvider"/>.
        /// </summary>
        public Wf WaveFormat => new Wf(44100, 16, 2);

        /// <summary>
        /// Indicates recorded data is available.
        /// </summary>
        public event EventHandler<DataAvailableEventArgs> DataAvailable;
        
        /// <summary>
        /// Frees up the resources used by this instant.
        /// </summary>
        public void Dispose()
        {
            lock (_syncLock)
            {
                ManagedBass.Bass.StreamFree(_mixer);

                foreach (var rec in _devices.Values)
                {
                    if (rec.RecordingHandle != 0)
                        ManagedBass.Bass.StreamFree(rec.RecordingHandle);

                    if (rec.SilenceHandle != 0)
                        ManagedBass.Bass.StreamFree(rec.SilenceHandle);
                }

                _running = false;

                _buffers.Clear();
            }
        }

        /// <summary>
        /// Start Recording.
        /// </summary>
        public void Start()
        {
            lock (_syncLock)
            {
                foreach (var rec in _devices.Values)
                {
                    if (rec.SilenceHandle != 0)
                        ManagedBass.Bass.ChannelPlay(rec.SilenceHandle);

                    if (rec.RecordingHandle != 0)
                        ManagedBass.Bass.ChannelPlay(rec.RecordingHandle);
                }

                ManagedBass.Bass.ChannelPlay(_mixer);

                _running = true;
            }
        }

        /// <summary>
        /// Stop Recording.
        /// </summary>
        public void Stop()
        {
            lock (_syncLock)
            {
                ManagedBass.Bass.ChannelPause(_mixer);

                foreach (var rec in _devices.Values)
                {
                    if (rec.RecordingHandle != 0)
                        ManagedBass.Bass.ChannelPause(rec.RecordingHandle);

                    if (rec.SilenceHandle != 0)
                        ManagedBass.Bass.ChannelPause(rec.SilenceHandle);
                }

                _running = false;
            }
        }
    }
}
