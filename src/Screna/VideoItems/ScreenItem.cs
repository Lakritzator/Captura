using System;
using System.Drawing;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Video;
using Screna.ImageProviders;

namespace Screna.VideoItems
{
    public class ScreenItem : NotifyPropertyChanged, IVideoItem
    {
        public IScreen Screen { get; }

        public ScreenItem(IScreen screen)
        {
            Screen = screen;
        }

        public string Name => Screen.DeviceName;

        public override string ToString() => Name;

        public IImageProvider GetImageProvider(bool includeCursor, out Func<Point, Point> transform)
        {
            transform = point => new Point(point.X - Screen.Rectangle.X, point.Y - Screen.Rectangle.Y);

            return new RegionProvider(Screen.Rectangle, includeCursor);
        }
    }
}