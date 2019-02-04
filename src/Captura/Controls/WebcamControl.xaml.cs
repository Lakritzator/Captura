using System.Windows;
using System.Windows.Interop;
using Captura.Models;
using Captura.Presentation;
using Captura.WebCam;
using Captura.Windows;
using Point = System.Drawing.Point;

namespace Captura.Controls
{
    public partial class WebCamControl
    {
        public CaptureWebCam Capture { get; private set; }

        public Filter VideoDevice { get; set; }

        public WebCamControl()
        {
            InitializeComponent();
        }

        public void Refresh()
        {
            // To change the video device, a dispose is needed.
            if (Capture != null)
            {
                Capture.Dispose();
                Capture = null;
            }

            // Create capture object.
            if (VideoDevice != null && PresentationSource.FromVisual(this) is HwndSource source)
            {
                Capture = new CaptureWebCam(VideoDevice, OpenPreview, source.Handle)
                {
                    Scale = Dpi.X
                };
                
                SizeChanged += (sender, e) => OnSizeChange();

                if (IsVisible)
                    Capture.StartPreview();

                OnSizeChange();
            }
        }

        public void ShowOnMainWindow(Window mainWindow)
        {
            // To change the video device, a dispose is needed.
            if (Capture != null)
            {
                Capture.Dispose();
                Capture = null;
            }

            // Create capture object.
            if (VideoDevice != null && PresentationSource.FromVisual(mainWindow) is HwndSource source)
            {
                Capture = new CaptureWebCam(VideoDevice, OpenPreview, source.Handle)
                {
                    Scale = Dpi.X
                };
                
                Capture.StartPreview();

                Capture.OnPreviewWindowResize(50, 40, new Point(280, 1));
            }
        }

        private void OnSizeChange()
        {
            Capture?.OnPreviewWindowResize(ActualWidth, ActualHeight, new Point(5, 40));
        }

        private void WebCamControl_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                Refresh();
            }
            else ShowOnMainWindow(Windows.MainWindow.Instance);
        }

        private void OpenPreview()
        {
            WebCamWindow.Instance.ShowAndFocus();
        }
    }
}
