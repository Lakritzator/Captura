using System.Windows;
using Captura.ImageEditor;
using Captura.Presentation;
using Captura.Windows;

namespace Captura.Pages
{
    public partial class MainPage
    {
        private void OpenCanvas(object sender, RoutedEventArgs e)
        {
            new ImageEditorWindow().ShowAndFocus();
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            SettingsWindow.ShowInstance();
        }
    }
}