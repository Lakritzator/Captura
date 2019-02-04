using System.Windows;
using Captura.Presentation;

namespace Captura.Windows
{
    public partial class FFmpegLogWindow
    {
        private FFmpegLogWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private static FFmpegLogWindow _instance;

        public static void ShowInstance()
        {
            if (_instance == null)
            {
                _instance = new FFmpegLogWindow();

                _instance.Closed += (sender, e) => _instance = null;
            }

            _instance.ShowAndFocus();
        }
    }
}
