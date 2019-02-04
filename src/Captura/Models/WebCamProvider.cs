using System;
using System.Collections.ObjectModel;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Services;
using Captura.WebCam;
using Captura.Windows;

namespace Captura.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WebCamProvider : NotifyPropertyChanged, IWebCamProvider
    {
        public WebCamProvider()
        {
            AvailableCams = new ReadOnlyObservableCollection<IWebCamItem>(_cams);

            _camControl = WebCamWindow.Instance.GetWebCamControl();
            
            Refresh();
        }

        private readonly ObservableCollection<IWebCamItem> _cams = new ObservableCollection<IWebCamItem>();

        public ReadOnlyObservableCollection<IWebCamItem> AvailableCams { get; }

        private readonly Controls.WebCamControl _camControl;

        private IWebCamItem _selectedCam = WebCamItem.NoWebCam;

        public IWebCamItem SelectedCam
        {
            get => _selectedCam;
            set
            {
                if (_selectedCam == value)
                    return;

                _selectedCam = value;

                _camControl.Capture?.StopPreview();

                if (value is WebCamItem model)
                {
                    try
                    {
                        _camControl.VideoDevice = model.Cam;

                        if (_camControl.IsVisible)
                            _camControl.Refresh();
                        else _camControl.ShowOnMainWindow(Windows.MainWindow.Instance);

                        _selectedCam = value;

                        OnPropertyChanged();
                    }
                    catch (Exception e)
                    {
                        ServiceProvider.MessageProvider.ShowException(e, "Could not Start WebCam");
                    }
                }

                OnPropertyChanged();
            }
        }
        
        public void Refresh()
        {
            _cams.Clear();

            _cams.Add(WebCamItem.NoWebCam);

            if (_camControl == null)
                return;

            foreach (var cam in Filter.VideoInputDevices)
                _cams.Add(new WebCamItem(cam));

            SelectedCam = WebCamItem.NoWebCam;
        }

        public IDisposable Capture(IBitmapLoader bitmapLoader)
        {
            try
            {
                return _camControl.Dispatcher.Invoke(() => _camControl.Capture?.GetFrame(bitmapLoader));
            }
            catch { return null; }
        }

        public int Width => _camControl.Dispatcher.Invoke(() => _camControl.Capture?.Size.Width ?? 0);

        public int Height => _camControl.Dispatcher.Invoke(() => _camControl.Capture?.Size.Height ?? 0);
    }
}
