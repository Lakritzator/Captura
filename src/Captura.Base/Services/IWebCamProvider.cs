using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Captura.Base.Images;

namespace Captura.Base.Services
{
    public interface IWebCamProvider : INotifyPropertyChanged
    {
        ReadOnlyObservableCollection<IWebCamItem> AvailableCams { get; }

        IWebCamItem SelectedCam { get; set; }

        void Refresh();

        IDisposable Capture(IBitmapLoader bitmapLoader);

        int Width { get; }

        int Height { get; }
    }
}
