using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Captura.Base;
using Captura.Base.Audio;
using Captura.Base.Images;
using Captura.Base.Services;
using Captura.Core.Models.Notifications;
using Captura.Loc;
using DesktopDuplication;
using Screna;
using Screna.VideoItems;
using Screna.VideoSourceProviders;

namespace Captura.Core.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ScreenShotModel : NotifyPropertyChanged
    {
        readonly VideoSourcesViewModel _videoSourcesViewModel;
        readonly ISystemTray _systemTray;
        readonly IRegionProvider _regionProvider;
        readonly IMainWindow _mainWindow;
        readonly IVideoSourcePicker _sourcePicker;
        readonly IAudioPlayer _audioPlayer;
        readonly Settings.Settings _settings;
        readonly LanguageManager _loc;
        readonly IPlatformServices _platformServices;

        public IReadOnlyList<IImageWriterItem> AvailableImageWriters { get; }

        public ScreenShotModel(VideoSourcesViewModel videoSourcesViewModel,
            ISystemTray systemTray,
            IRegionProvider regionProvider,
            IMainWindow mainWindow,
            IVideoSourcePicker sourcePicker,
            IAudioPlayer audioPlayer,
            IEnumerable<IImageWriterItem> imageWriters,
            Settings.Settings settings,
            LanguageManager loc,
            IPlatformServices platformServices)
        {
            _videoSourcesViewModel = videoSourcesViewModel;
            _systemTray = systemTray;
            _regionProvider = regionProvider;
            _mainWindow = mainWindow;
            _sourcePicker = sourcePicker;
            _audioPlayer = audioPlayer;
            _settings = settings;
            _loc = loc;
            _platformServices = platformServices;

            AvailableImageWriters = imageWriters.ToList();

            if (!AvailableImageWriters.Any(imageWriterItem => imageWriterItem.Active))
                AvailableImageWriters[0].Active = true;
        }

        public async Task ScreenshotRegion()
        {
            var region = _sourcePicker.PickRegion();

            if (region == null)
                return;

            await SaveScreenShot(ScreenShot.Capture(region.Value));
        }

        public async Task ScreenshotWindow()
        {
            var window = _sourcePicker.PickWindow();

            if (window != null)
            {
                var img = ScreenShot.Capture(window.Rectangle);

                await SaveScreenShot(img);
            }
        }

        public async Task ScreenshotScreen()
        {
            var screen = _sourcePicker.PickScreen();

            if (screen != null)
            {
                var img = ScreenShot.Capture(screen.Rectangle);

                await SaveScreenShot(img);
            }
        }

        public async Task SaveScreenShot(IBitmapImage bmp, string fileName = null)
        {
            _audioPlayer.Play(SoundKind.Shot);

            if (bmp != null)
            {
                var allTasks = AvailableImageWriters
                    .Where(imageWriterItem => imageWriterItem.Active)
                    .Select(imageWriterItem => imageWriterItem.Save(bmp, _settings.ScreenShots.ImageFormat, fileName));

                await Task.WhenAll(allTasks).ContinueWith(task => bmp.Dispose());
            }
            else _systemTray.ShowNotification(new TextNotification(_loc.ImgEmpty));
        }

        public IBitmapImage ScreenShotWindow(IWindow window)
        {
            _systemTray.HideNotification();

            if (window.Handle == _platformServices.DesktopWindow.Handle)
            {
                return ScreenShot.Capture(_settings.IncludeCursor);
            }

            try
            {
                IBitmapImage bmp = null;

                if (_settings.ScreenShots.WindowShotTransparent)
                {
                    bmp = ScreenShot.CaptureTransparent(window, _settings.IncludeCursor);
                }

                // Capture without Transparency
                return bmp ?? ScreenShot.Capture(window.Rectangle, _settings.IncludeCursor);
            }
            catch
            {
                return null;
            }
        }

        public async void CaptureScreenShot(string fileName = null)
        {
            _systemTray.HideNotification();

            var bmp = await GetScreenShot();

            await SaveScreenShot(bmp, fileName);
        }

        public async Task<IBitmapImage> GetScreenShot()
        {
            IBitmapImage bmp = null;

            var selectedVideoSource = _videoSourcesViewModel.SelectedVideoSourceKind?.Source;
            var includeCursor = _settings.IncludeCursor;

            switch (_videoSourcesViewModel.SelectedVideoSourceKind)
            {
                case WindowSourceProvider _:
                    var hWnd = _platformServices.DesktopWindow;

                    switch (selectedVideoSource)
                    {
                        case WindowItem windowItem:
                            hWnd = windowItem.Window;
                            break;
                    }

                    bmp = ScreenShotWindow(hWnd);
                    break;

                case DesktopDuplicationSourceProvider _:
                    if (selectedVideoSource is DesktopDuplicationItem deskDuplItem)
                    {
                        bmp = ScreenShot.Capture(deskDuplItem.Rectangle, includeCursor);
                    }
                    break;

                case FullScreenSourceProvider _:
                    var hide = _mainWindow.IsVisible && _settings.Ui.HideOnFullScreenShot;

                    if (hide)
                    {
                        _mainWindow.IsVisible = false;

                        // Ensure that the Window is hidden
                        await Task.Delay(300);
                    }

                    bmp = ScreenShot.Capture(includeCursor);

                    if (hide)
                        _mainWindow.IsVisible = true;
                    break;

                case ScreenSourceProvider _:
                    if (selectedVideoSource is ScreenItem screen)
                        bmp = ScreenShot.Capture(screen.Screen.Rectangle, includeCursor);
                    break;

                case RegionSourceProvider _:
                    bmp = ScreenShot.Capture(_regionProvider.SelectedRegion, includeCursor);
                    break;
            }

            return bmp;
        }
    }
}