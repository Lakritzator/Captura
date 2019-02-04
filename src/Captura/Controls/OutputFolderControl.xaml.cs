using System.Windows.Input;
using Captura.Core;
using Captura.ViewCore.ViewModels;

namespace Captura.Controls
{
    public partial class OutputFolderControl
    {
        public OutputFolderControl()
        {
            InitializeComponent();
        }

        private void SelectTargetFolder(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SelectOutputFolderCommand.ExecuteIfCan();
            }
        }
    }
}
