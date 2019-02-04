using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Captura.Base;
using Captura.HotKeys;
using Captura.Presentation;
using Button = System.Windows.Controls.Button;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Captura.Controls
{
    public class HotKeySelector : Button
    {
        private bool _editing;

        private static readonly SolidColorBrush RedBrush = new SolidColorBrush(WpfExtensions.ParseColor("#ef5350"));
        private static readonly SolidColorBrush GreenBrush = new SolidColorBrush(WpfExtensions.ParseColor("#43a047"));

        private static readonly SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);

        public static readonly DependencyProperty HotKeyModelProperty = DependencyProperty.Register(nameof(HotKeyModel),
            typeof(HotKey),
            typeof(HotKeySelector),
            new UIPropertyMetadata(HotKeyModelChangedCallback));

        private static void HotKeyModelChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (!(sender is HotKeySelector selector) || !(args.NewValue is HotKey hotKey))
            {
                return;
            }

            selector.TextColor();

            hotKey.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(HotKey.IsActive))
                    selector.TextColor();
            };

            selector.Content = hotKey.ToString();
        }

        public HotKey HotKeyModel
        {
            get => (HotKey) GetValue(HotKeyModelProperty);
            set => SetValue(HotKeyModelProperty, value);
        }

        private void HotKeyEdited(Key newKey, Modifiers newModifiers)
        {
            HotKeyEdited((Keys) KeyInterop.VirtualKeyFromKey(newKey), newModifiers);
        }

        private void TextColor()
        {
            if (HotKeyModel.IsActive)
            {
                Background = HotKeyModel.IsRegistered ? GreenBrush : RedBrush;

                Foreground = WhiteBrush;
            }
            else
            {
                ClearValue(BackgroundProperty);

                ClearValue(ForegroundProperty);
            }
        }

        private void HotKeyEdited(Keys newKey, Modifiers newModifiers)
        {
            HotKeyModel.Change(newKey, newModifiers);

            // Red Text on Error
            TextColor();

            Content = HotKeyModel.ToString();

            _editing = false;
        }
        
        protected override void OnClick()
        {
            base.OnClick();

            _editing = !_editing;

            Content = _editing ? "Press new HotKey..." : HotKeyModel.ToString();
        }

        protected override void OnLostFocus(RoutedEventArgs routedEventArgs)
        {
            base.OnLostFocus(routedEventArgs);

            CancelEditing();
        }

        private void CancelEditing()
        {
            if (!_editing)
                return;

            _editing = false;
            Content = HotKeyModel.ToString();
        }

        private static bool IsValid(KeyEventArgs keyEventArgs)
        {
            return keyEventArgs.Key != Key.None // Some key must pe pressed
                && !keyEventArgs.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Windows) // Windows Key is reserved by OS
                && keyEventArgs.Key != Key.LeftCtrl && keyEventArgs.Key != Key.RightCtrl // Modifier Keys alone are not supported
                && keyEventArgs.Key != Key.LeftAlt && keyEventArgs.Key != Key.RightAlt
                && keyEventArgs.Key != Key.LeftShift && keyEventArgs.Key != Key.RightShift;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs keyEventArgs)
        {
            // Ignore Repeats
            if (keyEventArgs.IsRepeat)
            {
                keyEventArgs.Handled = true;
                return;
            }

            if (_editing)
            {
                // Suppress event propagation
                keyEventArgs.Handled = true;

                switch (keyEventArgs.Key)
                {
                    case Key.Escape:
                        CancelEditing();
                        break;

                    case Key.System:
                        if (keyEventArgs.SystemKey == Key.LeftAlt || keyEventArgs.SystemKey == Key.RightAlt)
                            Content = "Alt + ...";
                        else HotKeyEdited(keyEventArgs.SystemKey, Modifiers.Alt);
                        break;

                    default:
                        if (IsValid(keyEventArgs))
                            HotKeyEdited(keyEventArgs.Key, (Modifiers)keyEventArgs.KeyboardDevice.Modifiers);

                        else
                        {
                            var modifiers = keyEventArgs.KeyboardDevice.Modifiers;

                            Content = "";

                            if (modifiers.HasFlag(ModifierKeys.Control))
                                Content += "Ctrl + ";

                            if (modifiers.HasFlag(ModifierKeys.Alt))
                                Content += "Alt + ";

                            if (modifiers.HasFlag(ModifierKeys.Shift))
                                Content += "Shift + ";

                            Content += "...";
                        }
                        break;
                }
            }

            base.OnPreviewKeyDown(keyEventArgs);
        }

        protected override void OnPreviewKeyUp(KeyEventArgs keyEventArgs)
        {
            // Ignore Repeats
            if (keyEventArgs.IsRepeat)
                return;

            if (_editing)
            {
                // Suppress event propagation
                keyEventArgs.Handled = true;

                // PrintScreen is not recognized in KeyDown
                switch (keyEventArgs.Key)
                {
                    case Key.Snapshot:
                        HotKeyEdited(Keys.PrintScreen, (Modifiers)keyEventArgs.KeyboardDevice.Modifiers);
                        break;

                    case Key.System when keyEventArgs.SystemKey == Key.Snapshot:
                        HotKeyEdited(Keys.PrintScreen, Modifiers.Alt);
                        break;
                }
            }

            base.OnPreviewKeyUp(keyEventArgs);
        }
    }
}