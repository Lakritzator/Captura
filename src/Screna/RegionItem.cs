using System;
using System.Drawing;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Services;
using Captura.Base.Video;
using Screna.ImageProviders;

namespace Screna
{
    public class RegionItem : NotifyPropertyChanged, IVideoItem
    {
        private readonly IRegionProvider _selector;

        public RegionItem(IRegionProvider regionSelector)
        {
            _selector = regionSelector;
        }

        public IImageProvider GetImageProvider(bool includeCursor, out Func<Point, Point> transform)
        {
            transform = point =>
            {
                var region = _selector.SelectedRegion.Location;

                return new Point(point.X - region.X, point.Y - region.Y);
            };

            return new RegionProvider(_selector.SelectedRegion, includeCursor,
                () => _selector.SelectedRegion.Location);
        }

        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                
                OnPropertyChanged();
            }
        }

        public override string ToString() => Name;
    }
}
