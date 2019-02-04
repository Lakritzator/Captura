using System;
using System.Threading.Tasks;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Recent;
using Captura.Base.Services;
using Captura.Core.Models.Recents;
using Captura.Loc;
using Screna;

namespace Captura.Core.Models.ImageWriterItems
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DiskWriter : NotifyPropertyChanged, IImageWriterItem
    {
        readonly ISystemTray _systemTray;
        readonly IMessageProvider _messageProvider;
        readonly Settings.Settings _settings;
        readonly LanguageManager _loc;
        readonly IRecentList _recentList;

        public DiskWriter(ISystemTray systemTray,
            IMessageProvider messageProvider,
            Settings.Settings settings,
            LanguageManager loc,
            IRecentList recentList)
        {
            _systemTray = systemTray;
            _messageProvider = messageProvider;
            _settings = settings;
            _loc = loc;
            _recentList = recentList;

            loc.LanguageChanged += cultureInfo => RaisePropertyChanged(nameof(Display));
        }

        public Task Save(IBitmapImage image, ImageFormats format, string fileName)
        {
            try
            {
                _settings.EnsureOutPath();

                var extension = format.ToString().ToLower();

                var saveFileName = _settings.GetFileName(extension, fileName);

                image.Save(saveFileName, format);
                
                _recentList.Add(new FileRecentItem(saveFileName, RecentFileType.Image));

                // Copy path to clipboard only when clipboard writer is off
                if (_settings.CopyOutPathToClipboard && !ServiceProvider.Get<ClipboardWriter>().Active)
                    saveFileName.WriteToClipboard();

                _systemTray.ShowScreenShotNotification(saveFileName);
            }
            catch (Exception e)
            {
                _messageProvider.ShowException(e, _loc.NotSaved);
            }

            return Task.CompletedTask;
        }

        public string Display => _loc.Disk;

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
