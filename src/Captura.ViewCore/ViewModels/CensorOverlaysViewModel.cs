using Captura.Core.Settings;
using Screna.Overlays.Settings;

namespace Captura.ViewCore.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CensorOverlaysViewModel : OverlayListViewModel<CensorOverlaySettings>
    {
        public CensorOverlaysViewModel(Settings settings) : base(settings.Censored)
        {
        }
    }
}