using System;
using System.ComponentModel;
using System.Drawing;
using Captura.Base.Images;

namespace Captura.Base.Video
{
    /// <summary>
    /// Items to show in Video Source List.
    /// </summary>
    public interface IVideoItem : INotifyPropertyChanged
    {
        string Name { get; }

        IImageProvider GetImageProvider(bool includeCursor, out Func<Point, Point> transform);
    }
}
