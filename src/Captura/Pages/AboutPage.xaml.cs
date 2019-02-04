using System.Windows;
using Captura.Base.Images;
using Captura.Base.Services;
using Captura.Core;
using Captura.ImageEditor;
using Captura.Presentation;
using Captura.Windows;
using Microsoft.Win32;

namespace Captura.Pages
{
    public partial class AboutPage
    {
        private void ViewLicenses(object sender, RoutedEventArgs e)
        {
            LicensesWindow.ShowInstance();
        }

        private void ViewCrashLogs(object sender, RoutedEventArgs e)
        {
            CrashLogsWindow.ShowInstance();
        }

        private void OpenImageEditor(object sender, RoutedEventArgs e)
        {
            new ImageEditorWindow().ShowAndFocus();
        }

        private void OpenAudioVideoTrimmer(object sender, RoutedEventArgs e)
        {
            new TrimmerWindow().ShowAndFocus();
        }

        private void OpenImageCropper(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.wmp;*.tiff",
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (ofd.ShowDialog().GetValueOrDefault())
            {
                new Windows.CropWindow(ofd.FileName).ShowAndFocus();
            }
        }

        private async void UploadToImgur(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.wmp;*.tiff",
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (ofd.ShowDialog().GetValueOrDefault())
            {
                var imgSystem = ServiceProvider.Get<IImagingSystem>();

                using (var img = imgSystem.LoadBitmap(ofd.FileName))
                {
                    await img.UploadImage();
                }
            }
        }
    }
}
