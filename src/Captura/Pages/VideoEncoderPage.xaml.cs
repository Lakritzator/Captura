using System.Windows;
using Captura.Presentation;
using Captura.ViewCore.ViewModels;

namespace Captura.Pages
{
    public partial class VideoEncoderPage
    {
        public VideoEncoderPage()
        {
            InitializeComponent();

            if (DataContext is MainViewModel vm)
            {
                vm.Refreshed += () => VideoWriterComboBox.Shake();
            }
        }

        private void OpenFFmpegLog(object sender, RoutedEventArgs e)
        {
            Windows.FFmpegLogWindow.ShowInstance();
        }
    }
}
