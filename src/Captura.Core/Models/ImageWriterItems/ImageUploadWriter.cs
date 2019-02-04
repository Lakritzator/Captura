using System;
using System.Threading.Tasks;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Recent;
using Captura.Base.Services;
using Captura.Core.Models.Notifications;
using Captura.Core.Models.Recents;
using Captura.Loc;
using Screna;

namespace Captura.Core.Models.ImageWriterItems
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ImageUploadWriter : NotifyPropertyChanged, IImageWriterItem
    {
        readonly DiskWriter _diskWriter;
        readonly ISystemTray _systemTray;
        readonly IMessageProvider _messageProvider;
        readonly Settings.Settings _settings;
        readonly LanguageManager _loc;
        readonly IRecentList _recentList;

        readonly IImageUploader _imgUploader;

        public ImageUploadWriter(DiskWriter diskWriter,
            ISystemTray systemTray,
            IMessageProvider messageProvider,
            Settings.Settings settings,
            LanguageManager languageManager,
            IRecentList recentList,
            IImageUploader imgUploader)
        {
            _imgUploader = imgUploader;

            _diskWriter = diskWriter;
            _systemTray = systemTray;
            _messageProvider = messageProvider;
            _settings = settings;
            _loc = languageManager;
            _recentList = recentList;

            languageManager.LanguageChanged += cultureInfo => RaisePropertyChanged(nameof(Display));
        }

        public async Task Save(IBitmapImage image, ImageFormats format, string fileName)
        {
            var response = await Save(image, format);

            switch (response)
            {
                case UploadResult uploadResult:
                    var link = uploadResult.Url;

                    // Copy path to clipboard only when clipboard writer is off
                    if (_settings.CopyOutPathToClipboard && !ServiceProvider.Get<ClipboardWriter>().Active)
                        link.WriteToClipboard();
                    break;

                case Exception e:
                    if (!_diskWriter.Active)
                    {
                        ServiceProvider.Get<IMainWindow>().IsVisible = true;

                        var yes = _messageProvider.ShowYesNo(
                            $"{e.Message}\n\nDo you want to Save to Disk?", _loc.ImageUploadFailed);

                        if (yes)
                            await _diskWriter.Save(image, format, fileName);
                    }
                    break;
            }
        }

        // Returns UploadResult on success, Exception on failure
        public async Task<object> Save(IBitmapImage image, ImageFormats format)
        {
            var progressItem = new ImageUploadNotification();
            _systemTray.ShowNotification(progressItem);

            try
            {
                var uploadResult = await _imgUploader.Upload(image, format, progressItemProgress => progressItem.Progress = progressItemProgress);

                var link = uploadResult.Url;

                _recentList.Add(new UploadRecentItem(link, uploadResult.DeleteLink, _imgUploader));

                progressItem.RaiseFinished(link);

                return uploadResult;
            }
            catch (Exception e)
            {
                progressItem.RaiseFailed();

                return e;
            }
        }

        public string Display => "Upload";

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
