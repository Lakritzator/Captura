using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Captura.Base;
using Captura.Base.Recent;
using Captura.Base.Services;
using Captura.Loc;
using Screna;

namespace Captura.Core.Models.Recents
{
    public class UploadRecentItem : IRecentItem
    {
        public string DeleteHash { get; }
        public string Link { get; }
        public IImageUploader UploaderService { get; }

        public UploadRecentItem(string link, string deleteHash, IImageUploader uploaderService)
        {
            DeleteHash = deleteHash;
            UploaderService = uploaderService;

            ClickCommand = new DelegateCommand(() => ServiceProvider.LaunchFile(new ProcessStartInfo(link)));

            RemoveCommand = new DelegateCommand(() => RemoveRequested?.Invoke());

            var icons = ServiceProvider.Get<IIconSet>();
            var loc = ServiceProvider.Get<LanguageManager>();

            Display = Link = link;
            Icon = icons.Link;

            Actions = new[]
            {
                new RecentAction(loc.CopyPath, icons.Clipboard, () => Link.WriteToClipboard()),
                new RecentAction(loc.Delete, icons.Delete, OnDelete)
            };
        }

        async void OnDelete()
        {
            if (!ServiceProvider.MessageProvider.ShowYesNo($"Are you sure you want to Delete: {Link}?", "Confirm Deletion"))
                return;

            try
            {
                await UploaderService.DeleteUploadedFile(DeleteHash);
            }
            catch (Exception e)
            {
                ServiceProvider.MessageProvider.ShowException(e, $"Could not Delete: {Link}");

                return;
            }

            RemoveRequested?.Invoke();
        }

        public string Display { get; }

        public string Icon { get; }

        public string IconColor => "MediumPurple";

        bool IRecentItem.IsSaving => false;

        public ICommand ClickCommand { get; }

        public ICommand RemoveCommand { get; }

        public IEnumerable<RecentAction> Actions { get; }

        public event Action RemoveRequested;
    }
}