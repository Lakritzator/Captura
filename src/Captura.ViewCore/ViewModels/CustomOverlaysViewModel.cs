using Captura.Core.Settings;
using Screna.Overlays.Settings;

namespace Captura.ViewCore.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CustomOverlaysViewModel : OverlayListViewModel<CustomOverlaySettings>
    {
        public CustomOverlaysViewModel(Settings settings) : base(settings.TextOverlays)
        {
        }
    }
}