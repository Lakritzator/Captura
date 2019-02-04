using Captura.Base.Audio;

namespace Captura.Base.Services
{
    public interface IAudioPlayer
    {
        void Play(SoundKind soundKind);
    }
}