using System.Drawing;
using System.Windows;
using Captura.Base.Services;
using Captura.Core.ViewModels;
using Screna.Frames;

namespace Captura.Windows
{
    public partial class WebCamWindow
    {
        private WebCamWindow()
        {
            InitializeComponent();
            
            Closing += (sender, e) =>
            {
                Hide();

                e.Cancel = true;
            };
        }

        public static WebCamWindow Instance { get; } = new WebCamWindow();

        public Controls.WebCamControl GetWebCamControl() => WebCameraControl;

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private async void CaptureImage_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var img = ServiceProvider.Get<IWebCamProvider>().Capture(GraphicsBitmapLoader.Instance);
                
                // HACK: Change IWebCamProvider to return IBitmapImage
                if (img is Bitmap bmp)
                    await ServiceProvider.Get<ScreenShotModel>().SaveScreenShot(new DrawingImage(bmp));
            }
            catch { }
        }
    }
}