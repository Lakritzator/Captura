using Captura.Base.Services;
using Captura.WebCam;

namespace Captura.Fakes
{
    public class FakesModule : IModule
    {
        public void OnLoad(IBinder binder)
        {
            // WebCam Provider
            binder.Bind<IWebCamProvider, CoreWebCamProvider>();

            binder.Bind<IMessageProvider, FakeMessageProvider>();
            binder.Bind<IRegionProvider>(() => FakeRegionProvider.Instance);
            binder.Bind<ISystemTray, FakeSystemTray>();
            binder.Bind<IMainWindow, FakeWindowProvider>();
            binder.Bind<IPreviewWindow, FakePreviewWindow>();
            binder.Bind<IVideoSourcePicker>(() => FakeVideoSourcePicker.Instance);
            binder.Bind<IAudioPlayer, FakeAudioPlayer>();
        }

        public void Dispose() { }
    }
}