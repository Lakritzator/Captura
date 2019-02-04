using Captura.Presentation;

namespace Captura.Windows
{
    public partial class SettingsWindow
    {
        private SettingsWindow()
        {
            InitializeComponent();
        }

        private static SettingsWindow _instance;

        public static void ShowInstance()
        {
            if (_instance == null)
            {
                _instance = new SettingsWindow();

                _instance.Closed += (sender, e) => _instance = null;
            }

            _instance.ShowAndFocus();
        }
    }
}
