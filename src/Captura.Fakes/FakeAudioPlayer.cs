using Captura.Base.Audio;
using Captura.Base.Services;

namespace Captura.Fakes
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FakeAudioPlayer : IAudioPlayer
    {
        public void Play(SoundKind soundKind) { }
    }
}