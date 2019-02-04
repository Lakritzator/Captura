using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Recent;
using Captura.Base.Services;
using Captura.Loc;
using Screna;

namespace Captura.Core.Models.Recents
{
    public class FileRecentItem : NotifyPropertyChanged, IRecentItem
    {
        public string FileName { get; }
        public RecentFileType FileType { get; }

        public FileRecentItem(string fileName, RecentFileType fileType, bool isSaving = false)
        {
            FileName = fileName;
            FileType = fileType;
            IsSaving = isSaving;

            Display = Path.GetFileName(fileName);

            ClickCommand = new DelegateCommand(() => ServiceProvider.LaunchFile(new ProcessStartInfo(fileName)));

            RemoveCommand = new DelegateCommand(() => RemoveRequested?.Invoke());

            var icons = ServiceProvider.Get<IIconSet>();
            var loc = ServiceProvider.Get<LanguageManager>();
            var windowService = ServiceProvider.Get<IMainWindow>();

            Icon = GetIcon(fileType, icons);
            IconColor = GetColor(fileType);

            var list = new List<RecentAction>
            {
                new RecentAction(loc.CopyPath, icons.Clipboard, () => FileName.WriteToClipboard())
            };

            void AddTrimMedia()
            {
                list.Add(new RecentAction(loc.Trim, icons.Trim, () => windowService.TrimMedia(fileName)));
            }

            switch (fileType)
            {
                case RecentFileType.Image:
                    list.Add(new RecentAction(loc.CopyToClipboard, icons.Clipboard, OnCopyToClipboardExecute));
                    list.Add(new RecentAction(loc.UploadToImgur, icons.Upload, OnUploadToImgurExecute));
                    list.Add(new RecentAction(loc.Edit, icons.Pencil, () => windowService.EditImage(fileName)));
                    list.Add(new RecentAction(loc.Crop, icons.Crop, () => windowService.CropImage(fileName)));
                    break;

                case RecentFileType.Audio:
                    AddTrimMedia();
                    break;

                case RecentFileType.Video:
                    AddTrimMedia();
                    list.Add(new RecentAction("Upload to YouTube", icons.YouTube, () => windowService.UploadToYouTube(fileName)));
                    break;
            }

            list.Add(new RecentAction(loc.Delete, icons.Delete, OnDelete));

            Actions = list;
        }

        async void OnUploadToImgurExecute()
        {
            if (!File.Exists(FileName))
            {
                ServiceProvider.MessageProvider.ShowError("File not Found");

                return;
            }

            var imgSystem = ServiceProvider.Get<IImagingSystem>();

            using (var img = imgSystem.LoadBitmap(FileName))
            {
                await img.UploadImage();
            }
        }

        void OnCopyToClipboardExecute()
        {
            if (!File.Exists(FileName))
            {
                ServiceProvider.MessageProvider.ShowError("File not Found");

                return;
            }

            try
            {
                var clipboard = ServiceProvider.Get<IClipboardService>();

                var imgSystem = ServiceProvider.Get<IImagingSystem>();

                using (var img = imgSystem.LoadBitmap(FileName))
                {
                    clipboard.SetImage(img);
                }
            }
            catch (Exception e)
            {
                ServiceProvider.MessageProvider.ShowException(e, "Copy to Clipboard failed");
            }
        }

        void OnDelete()
        {
            if (File.Exists(FileName))
            {
                var platformServices = ServiceProvider.Get<IPlatformServices>();

                if (!platformServices.DeleteFile(FileName))
                    return;
            }

            // Remove from List
            RemoveRequested?.Invoke();
        }

        static string GetIcon(RecentFileType itemType, IIconSet icons)
        {
            switch (itemType)
            {
                case RecentFileType.Audio:
                    return icons.Music;

                case RecentFileType.Image:
                    return icons.Image;

                case RecentFileType.Video:
                    return icons.Video;
            }

            return null;
        }

        static string GetColor(RecentFileType itemType)
        {
            switch (itemType)
            {
                case RecentFileType.Audio:
                    return "DodgerBlue";

                case RecentFileType.Image:
                    return "YellowGreen";

                case RecentFileType.Video:
                    return "OrangeRed";
            }

            return null;
        }

        public string Display { get; }

        public string Icon { get; }
        public string IconColor { get; }

        bool _saving;

        public bool IsSaving
        {
            get => _saving;
            private set
            {
                _saving = value;

                OnPropertyChanged();
            }
        }

        public void Saved()
        {
            IsSaving = false;
        }

        public event Action RemoveRequested;

        public ICommand ClickCommand { get; }
        public ICommand RemoveCommand { get; }

        public IEnumerable<RecentAction> Actions { get; }
    }
}