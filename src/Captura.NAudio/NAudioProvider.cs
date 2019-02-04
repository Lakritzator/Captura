using System;
using Captura.Base.Audio;
using NAudio.Wave;
using WaveFormat = Captura.Base.Audio.WaveFormat.WaveFormat;
using Wf = NAudio.Wave.WaveFormat;
using WaveFormatEncoding = NAudio.Wave.WaveFormatEncoding;

namespace Captura.NAudio
{
    abstract class NAudioProvider : IAudioProvider
    {
        readonly IWaveIn _waveIn;

        protected NAudioProvider(IWaveIn waveIn)
        {
            _waveIn = waveIn;

            _waveIn.DataAvailable += (sender, e) =>
            {
                DataAvailable?.Invoke(this, new DataAvailableEventArgs(e.Buffer, e.BytesRecorded));
            };

            var wf = waveIn.WaveFormat;
            NAudioWaveFormat = wf;

            WaveFormat = wf.Encoding == WaveFormatEncoding.IeeeFloat
                ? WaveFormat.CreateIeeeFloatWaveFormat(wf.SampleRate, wf.Channels)
                : new WaveFormat(wf.SampleRate, wf.BitsPerSample, wf.Channels);
        }

        public virtual void Dispose()
        {
            _waveIn.Dispose();
        }

        public WaveFormat WaveFormat { get; }

        public Wf NAudioWaveFormat { get; }

        public virtual void Start()
        {
            _waveIn.StartRecording();
        }

        public virtual void Stop()
        {
            _waveIn.StopRecording();
        }

        public event EventHandler<DataAvailableEventArgs> DataAvailable;
    }
}