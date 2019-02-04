using Captura.Presentation;

namespace Captura.Windows
{
    public partial class CrashLogsWindow
    {
        private CrashLogsWindow()
        {
            InitializeComponent();
        }

        private static CrashLogsWindow _instance;

        public static void ShowInstance()
        {
            if (_instance == null)
            {
                _instance = new CrashLogsWindow();

                _instance.Closed += (sender, e) => _instance = null;
            }

            _instance.ShowAndFocus();
        }
    }
}
