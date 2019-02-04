using Captura.Base.Services;
using Captura.Base.Video;
using Captura.Loc;
using Screna.VideoItems;

namespace Screna.VideoSourceProviders
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FullScreenSourceProvider : VideoSourceProviderBase
    {
        public FullScreenSourceProvider(LanguageManager loc,
            IIconSet icons,
            // ReSharper disable once SuggestBaseTypeForParameter
            FullScreenItem fullScreenItem) : base(loc)
        {
            Source = fullScreenItem;
            Icon = icons.MultipleMonitor;
        }

        public override IVideoItem Source { get; }

        public override string Name => Loc.FullScreen;

        public override string Description { get; } = "Record Fullscreen.";

        public override string Icon { get; }

        public override string Serialize() => "";

        public override bool Deserialize(string serialized) => true;

        public override bool ParseCli(string arg)
        {
            return arg == "desktop";
        }
    }
}