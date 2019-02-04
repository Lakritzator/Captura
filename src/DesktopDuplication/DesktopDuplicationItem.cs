using System;
using System.Drawing;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Video;
using SharpDX.DXGI;

namespace DesktopDuplication
{
    public class DesktopDuplicationItem : NotifyPropertyChanged, IVideoItem
    {
        private readonly Output1 _output;

        public DesktopDuplicationItem(Output1 output)
        {
            _output = output;
        }

        public string Name => _output.Description.DeviceName;

        public override string ToString() => Name;

        public Rectangle Rectangle
        {
            get
            {
                var rawRect = _output.Description.DesktopBounds;

                return new Rectangle(rawRect.Left, rawRect.Top, rawRect.Right - rawRect.Left, rawRect.Bottom - rawRect.Top);
            }
        }

        public IImageProvider GetImageProvider(bool includeCursor, out Func<Point, Point> transform)
        {
            var rect = Rectangle;

            transform = point => new Point(point.X - rect.Left, point.Y - rect.Top);

            return new DesktopDuplicationImageProvider(_output, includeCursor);
        }
    }
}