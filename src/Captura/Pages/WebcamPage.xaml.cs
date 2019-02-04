using System.Windows;
using Captura.Base.Services;
using Captura.Presentation;
using Captura.ViewCore.ViewModels;
using Captura.Windows;

namespace Captura.Pages
{
    public partial class WebCamPage
    {
        public WebCamPage()
        {
            InitializeComponent();

            ServiceProvider.Get<MainViewModel>().Refreshed += () =>
            {
                WebCamComboBox.Shake();
            };
        }

        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            WebCamWindow.Instance.ShowAndFocus();
        }
    }
}
