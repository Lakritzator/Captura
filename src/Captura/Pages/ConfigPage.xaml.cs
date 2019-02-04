using System.Windows;
using Captura.Windows;

namespace Captura.Pages
{
    public partial class ConfigPage
    {
        private void OpenOverlayManager(object sender, RoutedEventArgs e)
        {
            OverlayWindow.ShowInstance();
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            SettingsWindow.ShowInstance();
        }
    }
}
