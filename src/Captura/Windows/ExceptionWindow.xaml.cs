using System;
using System.Windows;
using Captura.ViewModels;

namespace Captura.Windows
{
    public partial class ExceptionWindow
    {
        public ExceptionWindow(Exception exception, string message = null)
        {
            InitializeComponent();

            if (DataContext is ExceptionViewModel vm)
            {
                vm.Init(exception, message);
            }
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenFFmpegLog(object sender, RoutedEventArgs e)
        {
            FFmpegLogWindow.ShowInstance();
        }
    }
}
