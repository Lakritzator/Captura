using System;
using System.Drawing;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Video;
using Screna.ImageProviders;

namespace Screna.VideoItems
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FullScreenItem : NotifyPropertyChanged, IVideoItem
    {
        public override string ToString() => Name;

        public string Name => null;

        public IImageProvider GetImageProvider(bool includeCursor, out Func<Point, Point> transform)
        {
			var region = WindowProvider.DesktopRectangle;

			transform = point => new Point(point.X - region.X, point.Y - region.Y);

            return new RegionProvider(region, includeCursor);
		}
    }
}