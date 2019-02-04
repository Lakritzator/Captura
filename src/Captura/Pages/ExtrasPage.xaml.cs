using System.Windows;
using System.Windows.Media;
using Captura.Core.ViewModels;
using FirstFloor.ModernUI.Presentation;

namespace Captura.Pages
{
    public partial class ExtrasPage
    {
        private void SelectedAccentColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue != null && DataContext is ViewModelBase vm)
            {
                AppearanceManager.Current.AccentColor = e.NewValue.Value;

                vm.Settings.Ui.AccentColor = e.NewValue.Value.ToString();
            }
        }

        private void DarkThemeClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModelBase vm)
            {
                AppearanceManager.Current.ThemeSource = vm.Settings.Ui.DarkTheme
                    ? AppearanceManager.DarkThemeSource
                    : AppearanceManager.LightThemeSource;
            }
        }
    }
}
