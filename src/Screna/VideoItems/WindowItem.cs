using System;
using System.Drawing;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Video;
using Screna.ImageProviders;

namespace Screna.VideoItems
{
    public class WindowItem : NotifyPropertyChanged, IVideoItem
    {
        public IWindow Window { get; }

        public WindowItem(IWindow window)
        {
            Window = window;
            Name = window.Title;
        }

        public override string ToString() => Name;

        public string Name { get; }

        public IImageProvider GetImageProvider(bool includeCursor, out Func<Point, Point> transform)
        {
            if (!Window.IsAlive)
            {
                throw new WindowClosedException();
            }

            return new WindowProvider(Window, includeCursor, out transform);
        }
    }
}