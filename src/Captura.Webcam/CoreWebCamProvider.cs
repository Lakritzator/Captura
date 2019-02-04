using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Services;

namespace Captura.WebCam
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CoreWebCamProvider : NotifyPropertyChanged, IWebCamProvider
    {
        public CoreWebCamProvider()
        {
            AvailableCams = new ReadOnlyObservableCollection<IWebCamItem>(_cams);

            _previewForm = new Form();
            
            Refresh();
        }

        private readonly Form _previewForm;

        private CaptureWebCam _captureWebCam;

        private readonly ObservableCollection<IWebCamItem> _cams = new ObservableCollection<IWebCamItem>();

        public ReadOnlyObservableCollection<IWebCamItem> AvailableCams { get; }

        private IWebCamItem _selectedCam = WebCamItem.NoWebCam;

        public IWebCamItem SelectedCam
        {
            get => _selectedCam;
            set
            {
                if (_selectedCam == value)
                    return;

                _selectedCam = value;

                if (_captureWebCam != null)
                {
                    _captureWebCam.StopPreview();
                    _captureWebCam.Dispose();

                    _captureWebCam = null;
                }

                if (value is WebCamItem model)
                {
                    try
                    {
                        _captureWebCam = new CaptureWebCam(model.Cam, null, _previewForm.Handle);

                        _captureWebCam.StartPreview();

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

            foreach (var cam in Filter.VideoInputDevices)
            {
                _cams.Add(new WebCamItem(cam));
            }

            SelectedCam = WebCamItem.NoWebCam;
        }

        public IDisposable Capture(IBitmapLoader bitmapLoader)
        {
            try
            {
                return _captureWebCam?.GetFrame(bitmapLoader);
            }
            catch { return null; }
        }

        public int Width => _captureWebCam?.Size.Width ?? 0;

        public int Height => _captureWebCam?.Size.Height ?? 0;
    }
}
