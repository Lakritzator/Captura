using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Captura.Presentation;

namespace Captura.Windows
{
    public partial class CropWindow
    {
        private BitmapSource _croppedImage;
        private readonly string _fileName;

        public CropWindow(string fileName)
        {
            InitializeComponent();

            _fileName = fileName;

            using (var stream = File.OpenRead(fileName))
            {
                var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                Image.Source = decoder.Frames[0];
            }

            Loaded += (s, e) =>
            {
                var rcInterior = new Rect(
                    Image.ActualWidth * 0.2,
                    Image.ActualHeight * 0.2,
                    Image.ActualWidth * 0.6,
                    Image.ActualHeight * 0.6);

                var layer = AdornerLayer.GetAdornerLayer(Image);

                var croppingAdorner = new CroppingAdorner(Image, rcInterior);

                layer.Add(croppingAdorner);
                
                void RefreshCropImage()
                {
                    _croppedImage = croppingAdorner.BpsCrop(Image.Source as BitmapSource);

                    SizeLabel.Content = _croppedImage != null
                        ? $"{(int) _croppedImage.Width} x {(int) _croppedImage.Height}"
                        : "";
                }

                RefreshCropImage();

                croppingAdorner.CropChanged += (sender, args) => RefreshCropImage();

                croppingAdorner.Checked += Save;

                var clr = Colors.Black;
                clr.A = 110;
                croppingAdorner.Fill = new SolidColorBrush(clr);
            };
        }

        private void Save()
        {
            if (_croppedImage == null)
                return;

            if (_croppedImage.SaveToPickedFile(_fileName))
            {
                Close();
            }
        }
    }
}
