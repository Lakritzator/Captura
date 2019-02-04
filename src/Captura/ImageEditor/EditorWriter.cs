using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Captura.Base;
using Captura.Base.Images;

namespace Captura.ImageEditor
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class EditorWriter : NotifyPropertyChanged, IImageWriterItem
    {
        public Task Save(IBitmapImage image, ImageFormats format, string fileName)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, ImageFormats.Png);

                var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                var win = new ImageEditorWindow();

                win.Open(decoder.Frames[0]);

                win.Show();
            }

            return Task.CompletedTask;
        }

        public string Display => "Editor";

        private bool _active;

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;

                OnPropertyChanged();
            }
        }

        public override string ToString() => Display;
    }
}
