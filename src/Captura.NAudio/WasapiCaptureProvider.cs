using NAudio.CoreAudioApi;

namespace Captura.NAudio
{
    class WasapiCaptureProvider : NAudioProvider
    {
        public WasapiCaptureProvider(MMDevice device)
            : base(new WasapiCapture(device)) { }
    }
}