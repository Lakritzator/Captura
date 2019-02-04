using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Services;
using Captura.Core.Models;
using Captura.Core.Models.ImageWriterItems;
using Captura.Core.Settings;
using Captura.Core.ViewModels;
using Captura.Loc;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Captura.ViewCore.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ScreenShotViewModel : ViewModelBase
    {
        public DiskWriter DiskWriter { get; }
        public ClipboardWriter ClipboardWriter { get; }
        public ImageUploadWriter ImgurWriter { get; }

        public ScreenShotViewModel(LanguageManager loc,
            Settings settings,
            DiskWriter diskWriter,
            ClipboardWriter clipboardWriter,
            ImageUploadWriter imgurWriter,
            ScreenShotModel screenShotModel,
            VideoSourcesViewModel videoSourcesViewModel,
            IPlatformServices platformServices) : base(settings, loc)
        {
            DiskWriter = diskWriter;
            ClipboardWriter = clipboardWriter;
            ImgurWriter = imgurWriter;

            ScreenShotCommand = videoSourcesViewModel
                .ObserveProperty(sourcesViewModel => sourcesViewModel.SelectedVideoSourceKind)
                .Select(videoSourceProvider => !(videoSourceProvider is NoVideoSourceProvider))
                .ToReactiveCommand()
                .WithSubscribe(() => screenShotModel.CaptureScreenShot());

            async Task ScreenShotWindow(IWindow window)
            {
                var img = screenShotModel.ScreenShotWindow(window);

                await screenShotModel.SaveScreenShot(img);
            }

            ScreenShotActiveCommand = new DelegateCommand(async () => await ScreenShotWindow(platformServices.ForegroundWindow));
            ScreenShotDesktopCommand = new DelegateCommand(async () => await ScreenShotWindow(platformServices.DesktopWindow));
            ScreenshotRegionCommand = new DelegateCommand(async () => await screenShotModel.ScreenshotRegion());
            ScreenshotWindowCommand = new DelegateCommand(async () => await screenShotModel.ScreenshotWindow());
            ScreenshotScreenCommand = new DelegateCommand(async () => await screenShotModel.ScreenshotScreen());
        }

        public ICommand ScreenShotCommand { get; }
        public ICommand ScreenShotActiveCommand { get; }
        public ICommand ScreenShotDesktopCommand { get; }
        public ICommand ScreenshotRegionCommand { get; }
        public ICommand ScreenshotWindowCommand { get; }
        public ICommand ScreenshotScreenCommand { get; }

        public IEnumerable<ImageFormats> ScreenShotImageFormats { get; } = Enum
            .GetValues(typeof(ImageFormats))
            .Cast<ImageFormats>();
    }
}