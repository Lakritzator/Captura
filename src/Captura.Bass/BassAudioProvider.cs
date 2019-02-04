using System;
using System.Runtime.InteropServices;
using Captura.Base.Audio;
using ManagedBass;
using Wf = Captura.Base.Audio.WaveFormat.WaveFormat;

namespace Captura.Bass
{
    class BassAudioProvider : IAudioProvider
    {
        readonly int _recordingHandle;

        readonly int _silenceHandle;
 
        readonly object _syncLock = new object();

        public BassAudioProvider(BassItem device)
        {
            if (device == null)
                throw new ArgumentNullException();

            ManagedBass.Bass.RecordInit(device.Id);

            var devInfo = ManagedBass.Bass.RecordGetDeviceInfo(device.Id);

            if (devInfo.IsLoopback)
            {
                var playbackDevice = FindPlaybackDevice(devInfo);

                if (playbackDevice != -1)
                {
                    ManagedBass.Bass.Init(playbackDevice);
                    ManagedBass.Bass.CurrentDevice = playbackDevice;

                    _silenceHandle = ManagedBass.Bass.CreateStream(44100, 2, BassFlags.Default, Extensions.SilenceStreamProcedure);

                    ManagedBass.Bass.ChannelSetAttribute(_silenceHandle, ChannelAttribute.Volume, 0);
                }
            }

            ManagedBass.Bass.CurrentRecordingDevice = device.Id;

            var info = ManagedBass.Bass.RecordingInfo;

            _recordingHandle = ManagedBass.Bass.RecordStart(info.Frequency, info.Channels, BassFlags.RecordPause, RecordProcedure);
        }

        bool RecordProcedure(int handle, IntPtr ptr, int length, IntPtr user)
        {
            var buffer = GetBuffer(length);

            Marshal.Copy(ptr, buffer, 0, length);

            DataAvailable?.Invoke(this, new DataAvailableEventArgs(buffer, length));

            return true;
        }

        static int FindPlaybackDevice(DeviceInfo loopbackDeviceInfo)
        {
            for (var i = 0; ManagedBass.Bass.GetDeviceInfo(i, out var info); ++i)
            {
                if (info.Driver == loopbackDeviceInfo.Driver)
                    return i;
            }

            return -1;
        }

        byte[] _buffer;

        byte[] GetBuffer(int length)
        {
            if (_buffer == null || _buffer.Length < length)
            {
                _buffer = new byte[length + 1000];

                Console.WriteLine($"New Audio Buffer Allocated: {length}");
            }

            return _buffer;
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
                ManagedBass.Bass.StreamFree(_recordingHandle);

                if (_silenceHandle != 0)
                    ManagedBass.Bass.StreamFree(_silenceHandle);

                _buffer = null;
            }
        }

        /// <summary>
        /// Start Recording.
        /// </summary>
        public void Start()
        {
            lock (_syncLock)
            {
                if (_silenceHandle != 0)
                    ManagedBass.Bass.ChannelPlay(_silenceHandle);

                ManagedBass.Bass.ChannelPlay(_recordingHandle);
            }
        }

        /// <summary>
        /// Stop Recording.
        /// </summary>
        public void Stop()
        {
            lock (_syncLock)
            {
                ManagedBass.Bass.ChannelPause(_recordingHandle);

                if (_silenceHandle != 0)
                    ManagedBass.Bass.ChannelPause(_silenceHandle);
            }
        }
    }
}
