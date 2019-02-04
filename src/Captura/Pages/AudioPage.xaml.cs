using Captura.Base.Services;
using Captura.Presentation;
using Captura.ViewCore.ViewModels;

namespace Captura.Pages
{
    public partial class AudioPage
    {
        public AudioPage()
        {
            InitializeComponent();

            ServiceProvider.Get<MainViewModel>().Refreshed += () =>
            {
                AudioSourcesPanel.Shake();
            };
        }
    }
}
