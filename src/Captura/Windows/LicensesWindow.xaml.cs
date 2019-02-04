using Captura.Presentation;

namespace Captura.Windows
{
    public partial class LicensesWindow
    {
        private LicensesWindow()
        {
            InitializeComponent();
        }

        private static LicensesWindow _instance;

        public static void ShowInstance()
        {
            if (_instance == null)
            {
                _instance = new LicensesWindow();

                _instance.Closed += (sender, e) => _instance = null;
            }

            _instance.ShowAndFocus();
        }
    }
}
