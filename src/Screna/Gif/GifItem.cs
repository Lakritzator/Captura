using Captura.Base.Video;

namespace Screna.Gif
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class GifItem : IVideoWriterItem
    {
        private readonly GifSettings _settings;

        public GifItem(GifSettings settings)
        {
            _settings = settings;
        }

        public string Extension { get; } = ".gif";

        public string Description => "Encode GIF";

        public override string ToString() => "Gif";

        public IVideoFileWriter GetVideoFileWriter(VideoWriterArgs args)
        {
            var repeat = _settings.Repeat ? _settings.RepeatCount : -1;
            
            return new GifWriter(args.FileName, args.FrameRate, repeat);
        }
    }
}
