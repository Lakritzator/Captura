using Captura.Base;
using Captura.Base.Audio;
using Captura.Base.Audio.WaveFormat;

namespace Screna.VideoItems
{
    public class WaveItem : NoVideoItem
    {
        public static WaveItem Instance { get; } = new WaveItem();

        private WaveItem() : base("Wave", ".wav") { }

        public override IAudioFileWriter GetAudioFileWriter(string fileName, WaveFormat wf, int audioQuality)
        {
            return new AudioFileWriter(fileName, wf);
        }
    }
}
