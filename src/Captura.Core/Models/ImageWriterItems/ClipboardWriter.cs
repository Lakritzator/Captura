using System.Threading.Tasks;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Services;
using Captura.Core.Models.Notifications;
using Captura.Loc;

namespace Captura.Core.Models.ImageWriterItems
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ClipboardWriter : NotifyPropertyChanged, IImageWriterItem
    {
        readonly ISystemTray _systemTray;
        readonly IClipboardService _clipboard;
        readonly LanguageManager _loc;

        public ClipboardWriter(ISystemTray systemTray,
            LanguageManager loc,
            IClipboardService clipboard)
        {
            _systemTray = systemTray;
            _loc = loc;
            _clipboard = clipboard;

            loc.LanguageChanged += cultureInfo => RaisePropertyChanged(nameof(Display));
        }

        public Task Save(IBitmapImage image, ImageFormats format, string fileName)
        {
            _clipboard.SetImage(image);

            _systemTray.ShowNotification(new TextNotification(_loc.ImgSavedClipboard));

            return Task.CompletedTask;
        }

        public string Display => _loc.Clipboard;

        bool _active;

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
