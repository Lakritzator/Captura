using System.Windows;
using System.Windows.Input;
using Captura.Core;
using Captura.ViewCore;

namespace Captura.Pages
{
    public partial class SoundsPage
    {
        public SoundsPage()
        {
            InitializeComponent();
        }

        private void SetFile(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is SoundsViewModelItem vm)
            {
                vm.SetCommand.ExecuteIfCan();
            }
        }
    }
}
