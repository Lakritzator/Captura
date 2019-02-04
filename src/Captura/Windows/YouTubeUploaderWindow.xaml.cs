using System.Windows;
using Captura.YouTube;

namespace Captura.Windows
{
    public partial class YouTubeUploaderWindow
    {
        public YouTubeUploaderWindow()
        {
            InitializeComponent();

            Closing += async (sender, e) =>
            {
                if (DataContext is YouTubeUploaderViewModel vm)
                {
                    if (!await vm.Cancel())
                    {
                        e.Cancel = true;
                    }
                }
            };
        }

        public async void Open(string fileName)
        {
            if (DataContext is YouTubeUploaderViewModel vm)
            {
                await vm.Init(fileName);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
