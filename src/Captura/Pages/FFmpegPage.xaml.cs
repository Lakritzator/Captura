using System.Windows;
using System.Windows.Input;
using Captura.Core;
using Captura.FFmpeg;
using Captura.ViewCore.ViewModels;

namespace Captura.Pages
{
    public partial class FFmpegPage
    {
        private void OpenFFmpegLog(object sender, RoutedEventArgs e)
        {
            Windows.FFmpegLogWindow.ShowInstance();
        }

        private void FFmpegDownload(object sender, RoutedEventArgs e)
        {
            FFmpegService.FFmpegDownloader?.Invoke();
        }

        private void SelectFFmpegFolder(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SelectFFmpegFolderCommand.ExecuteIfCan();
            }
        }
    }
}
