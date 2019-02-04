using System.Windows;

namespace Captura.Controls
{
    public partial class ModernPasswordBox
    {
        public ModernPasswordBox()
        {
            InitializeComponent();

            PswBox.PasswordChanged += (sender, e) => Password = PswBox.Password;
        }

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
            nameof(Password),
            typeof(string),
            typeof(ModernPasswordBox),
            new UIPropertyMetadata((dependencyObject, e) =>
            {
                if (dependencyObject is ModernPasswordBox modernPswBox && e.NewValue is string psw)
                {
                    if (modernPswBox.PswBox.Password != psw)
                        modernPswBox.PswBox.Password = psw;
                }
            }));

        public string Password
        {
            get => (string) GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }
    }
}
