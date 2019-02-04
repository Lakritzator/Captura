using System.Drawing;
using System.Text.RegularExpressions;
using Captura.Base.Services;
using Captura.Base.Video;
using Captura.Loc;

namespace Screna.VideoSourceProviders
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RegionSourceProvider : VideoSourceProviderBase
    {
        private readonly IRegionProvider _regionProvider;
        private static readonly RectangleConverter RectangleConverter = new RectangleConverter();

        public RegionSourceProvider(LanguageManager loc,
            IRegionProvider regionProvider,
            IIconSet icons) : base(loc)
        {
            _regionProvider = regionProvider;

            Source = regionProvider.VideoSource;
            Icon = icons.Region;

            regionProvider.SelectorHidden += RequestUnselect;
        }

        public override IVideoItem Source { get; }

        public override string Name => Loc.Region;

        public override string Description { get; } = "Record region selected using Region Selector.";

        public override string Icon { get; }

        public override bool OnSelect()
        {
            _regionProvider.SelectorVisible = true;

            return true;
        }

        public override void OnUnselect()
        {
            _regionProvider.SelectorVisible = false;
        }

        public override string Serialize()
        {
            var rect = _regionProvider.SelectedRegion;
            return RectangleConverter.ConvertToInvariantString(rect);
        }

        public override bool Deserialize(string serialized)
        {
            if (!(RectangleConverter.ConvertFromInvariantString(serialized) is Rectangle rect))
                return false;

            _regionProvider.SelectedRegion = rect;

            OnSelect();

            return true;
        }

        public override bool ParseCli(string arg)
        {
            if (!Regex.IsMatch(arg, @"^\d+,\d+,\d+,\d+$"))
                return false;

            var rectConverter = new RectangleConverter();

            if (!(rectConverter.ConvertFromInvariantString(arg) is Rectangle rect))
                return false;

            _regionProvider.SelectedRegion = rect.Even();

            return true;
        }
    }
}