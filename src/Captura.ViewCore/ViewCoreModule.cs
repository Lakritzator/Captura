using Captura.Base.Services;
using Captura.ViewCore.ViewModels;

namespace Captura.ViewCore
{
    public class ViewCoreModule : IModule
    {
        public void OnLoad(IBinder binder)
        {
            binder.BindSingleton<CrashLogsViewModel>();
            binder.BindSingleton<FileNameFormatViewModel>();
            binder.BindSingleton<LicensesViewModel>();
            binder.BindSingleton<ProxySettingsViewModel>();
            binder.BindSingleton<SoundsViewModel>();
            binder.BindSingleton<RecentViewModel>();
            binder.BindSingleton<UpdateCheckerViewModel>();
            binder.BindSingleton<ScreenShotViewModel>();
            binder.BindSingleton<RecordingViewModel>();
            binder.BindSingleton<MainViewModel>();
            binder.BindSingleton<ViewConditionsModel>();

            binder.BindSingleton<CustomOverlaysViewModel>();
            binder.BindSingleton<CustomImageOverlaysViewModel>();
            binder.BindSingleton<CensorOverlaysViewModel>();
            binder.BindSingleton<HotKeyActionRegisterer>();
        }

        public void Dispose() { }
    }
}