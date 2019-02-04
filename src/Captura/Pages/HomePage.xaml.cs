using Captura.Presentation;
using Captura.ViewCore.ViewModels;

namespace Captura.Pages
{
    public partial class HomePage
    {
        public HomePage()
        {
            InitializeComponent();

            if (DataContext is MainViewModel vm)
            {
                vm.Refreshed += () => AudioDropdown.Shake();
            }
        }
    }
}
