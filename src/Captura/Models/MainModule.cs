using System;
using Captura.Base.Images;
using Captura.Base.Services;
using Captura.ImageEditor;
using Captura.ViewModels;
using Hardcodet.Wpf.TaskbarNotification;

namespace Captura.Models
{
    public class MainModule : IModule
    {
        public void OnLoad(IBinder binder)
        {
            // Use singleton to ensure the same instance is used every time.
            binder.Bind<IMessageProvider, MessageProvider>();
            binder.Bind<IRegionProvider, RegionSelectorProvider>();
            binder.Bind<ISystemTray, SystemTray>();
            binder.Bind<IPreviewWindow, PreviewWindowService>();
            binder.Bind<IVideoSourcePicker, VideoSourcePicker>();
            binder.Bind<IAudioPlayer, AudioPlayer>();

            binder.BindSingleton<EditorWriter>();
            binder.Bind<IImageWriterItem>(ServiceProvider.Get<EditorWriter>);

            binder.Bind<IWebCamProvider, WebCamProvider>();
            
            binder.BindSingleton<AboutViewModel>();
            binder.BindSingleton<RegionSelectorViewModel>();

            // Bind as a Function to ensure the UI objects are referenced only after they have been created.
            binder.Bind<Func<TaskbarIcon>>(() => () => Windows.MainWindow.Instance.SystemTray);
            binder.Bind<IMainWindow>(() => new MainWindowProvider(() => Windows.MainWindow.Instance));
        }

        public void Dispose() { }
    }
}