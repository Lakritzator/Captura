using System;
using System.Windows.Input;
using Captura.Base;
using Captura.Base.Services;
using Captura.Core.Settings;
using Screna.Overlays.Settings;

namespace Captura.ViewCore.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CustomImageOverlaysViewModel : OverlayListViewModel<CustomImageOverlaySettings>
    {
        private readonly IDialogService _dialogService;

        public CustomImageOverlaysViewModel(Settings settings, IDialogService dialogService) : base(settings.ImageOverlays)
        {
            _dialogService = dialogService;

            ChangeCommand = new DelegateCommand(Change);
        }

        public ICommand ChangeCommand { get; }

        private void Change(object o)
        {
            if (!(o is CustomImageOverlaySettings settings))
            {
                return;
            }

            var fileName = _dialogService.PickFile(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "Select Image");

            if (fileName != null)
            {
                settings.Source = fileName;
            }
        }
    }
}