using System.Collections;
using System.Collections.Generic;
using Captura.Base.Video;
using Captura.FFmpeg.Settings;

namespace Captura.FFmpeg.Video
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FFmpegWriterProvider : IVideoWriterProvider
    {
        public string Name => "FFmpeg";

        private readonly FFmpegSettings _settings;

        public FFmpegWriterProvider(FFmpegSettings settings)
        {
            _settings = settings;
        }

        public IEnumerator<IVideoWriterItem> GetEnumerator()
        {
            foreach (var codec in FFmpegItem.Encoders)
            {
                yield return codec;
            }

            yield return FFmpegGifItem.Instance;

            foreach (var codec in FFmpegItem.HardwareEncoders)
            {
                yield return codec;
            }

            foreach (var codec in FFmpegPostProcessingItem.Items)
            {
                yield return codec;
            }

            foreach (var item in _settings.CustomCodecs)
            {
                yield return new FFmpegItem(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => Name;

        public string Description => @"Use FFmpeg for encoding.
Requires ffmpeg.exe, if not found option for downloading or specifying path is shown.";
    }
}