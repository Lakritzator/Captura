using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using Captura.Core;
using Captura.FFmpeg;

namespace Captura.Windows
{
    public partial class FFmpegDownloaderWindow
    {
        public FFmpegDownloaderWindow()
        {
            InitializeComponent();

            if (!(DataContext is FFmpegDownloadViewModel vm))
            {
                return;
            }

            vm.CloseWindowAction += Close;

            vm.ProgressChanged += i =>
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                    TaskbarItemInfo.ProgressValue = i / 100.0;
                });
            };

            vm.AfterDownload += success =>
            {
                Dispatcher.Invoke(() =>
                {
                    TaskbarItemInfo.ProgressState = success ? TaskbarItemProgressState.None : TaskbarItemProgressState.Error;
                    TaskbarItemInfo.ProgressValue = 1;
                });
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void SelectTargetFolder(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is FFmpegDownloadViewModel vm)
            {
                vm.SelectFolderCommand.ExecuteIfCan();
            }
        }
    }
}
