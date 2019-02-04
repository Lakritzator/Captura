using System;
using System.Drawing;
using Captura.Base.Audio;
using Captura.Base.Audio.WaveFormat;
using Captura.Base.Images;
using Captura.Base.Video;

namespace Captura.Base
{
    /// <summary>
    /// Holds codecs for audio-alone capture.
    /// </summary>
    public abstract class NoVideoItem : NotifyPropertyChanged, IVideoItem
    {
        protected NoVideoItem(string displayName, string extension)
        {
            Name = displayName;

            Extension = extension;
        }

        public string Name { get; }

        public IImageProvider GetImageProvider(bool includeCursor, out Func<Point, Point> transform)
        {
            transform = null;

            return null;
        }

        public override string ToString() => Name;

        public string Extension { get; }

        public abstract IAudioFileWriter GetAudioFileWriter(string fileName, WaveFormat wf, int audioQuality);
    }
}
