using Captura.Base.Video;

namespace Captura.SharpAvi
{
    internal class SharpAviItem : IVideoWriterItem
    {
        private readonly AviCodec _codec;

        public SharpAviItem(AviCodec codec, string description)
        {
            _codec = codec;
            Description = description;
        }

        public string Extension { get; } = ".avi";

        public string Description { get; }

        public IVideoFileWriter GetVideoFileWriter(VideoWriterArgs args)
        {
            _codec.Quality = args.VideoQuality;

            return new AviWriter(args.FileName, _codec, args.ImageProvider, args.FrameRate, args.AudioProvider);
        }
        
        public override string ToString() => _codec.Name;
    }
}
