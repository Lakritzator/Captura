using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Services;
using Captura.Models;
using Screna;
using Color = System.Windows.Media.Color;
using Cursors = System.Windows.Input.Cursors;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Captura.Windows
{
    public partial class VideoSourcePickerWindow
    {
        private enum VideoPickerMode
        {
            Window,
            Screen
        }

        private readonly VideoPickerMode _mode;

        private Predicate<IWindow> Predicate { get; set; }

        private VideoSourcePickerWindow(VideoPickerMode mode)
        {
            _mode = mode;
            InitializeComponent();

            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            UpdateBackground();

            var platformServices = ServiceProvider.Get<IPlatformServices>();

            _screens = platformServices.EnumerateScreens().ToArray();
            _windows = platformServices.EnumerateWindows().ToArray();

            ShowCancelText();
        }

        private readonly IScreen[] _screens;

        private readonly IWindow[] _windows;

        public IScreen SelectedScreen { get; private set; }

        public IWindow SelectedWindow { get; private set; }

        private void UpdateBackground()
        {
            using (var bmp = ScreenShot.Capture())
            {
                var stream = new MemoryStream();
                bmp.Save(stream, ImageFormats.Png);

                stream.Seek(0, SeekOrigin.Begin);

                var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                Background = new ImageBrush(decoder.Frames[0]);
            }
        }

        private void BeginClose()
        {
            var duration = new Duration(TimeSpan.FromMilliseconds(200));

            var opacityAnim = new DoubleAnimation(0, duration);

            opacityAnim.Completed += (sender, e) => Close();

            BeginAnimation(OpacityProperty, opacityAnim);
        }

        private void ShowCancelText()
        {
            foreach (var screen in _screens)
            {
                var bounds = screen.Rectangle;

                var left = -Left + bounds.Left / Dpi.X;
                var top = -Top + bounds.Top / Dpi.Y;
                var width = bounds.Width / Dpi.X;
                var height = bounds.Height / Dpi.Y;

                var container = new ContentControl
                {
                    Width = width,
                    Height = height,
                    Margin = new Thickness(left, top, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                var textBlock = new TextBlock
                {
                    Text = $"Select {_mode} or Press Esc to Cancel",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(10, 5, 10, 5),
                    Foreground = new SolidColorBrush(Colors.White),
                    Background = new SolidColorBrush(Color.FromArgb(183, 0, 0, 0))
                };

                container.Content = textBlock;

                Grid.Children.Add(container);
            }
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            SelectedScreen = null;
            SelectedWindow = null;

            BeginClose();
        }

        private Rectangle? _lastRectangle;

        private void UpdateBorderAndCursor(Rectangle? rect)
        {
            if (_lastRectangle == rect)
                return;

            _lastRectangle = rect;

            var storyboard = new Storyboard();

            var duration = new Duration(TimeSpan.FromMilliseconds(100));

            if (rect == null)
            {
                Cursor = Cursors.Arrow;

                var opacityAnim = new DoubleAnimation(0, duration);
                Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(nameof(Opacity)));
                storyboard.Children.Add(opacityAnim);
            }
            else
            {
                Cursor = Cursors.Hand;

                var opacityAnim = new DoubleAnimation(1, duration);
                Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(nameof(Opacity)));
                storyboard.Children.Add(opacityAnim);

                var newRect = rect.Value;

                var margin = new Thickness(-Left + newRect.Left / Dpi.X, -Top + newRect.Top / Dpi.Y, 0, 0);
                var marginAnim = new ThicknessAnimation(margin, duration);
                Storyboard.SetTargetProperty(marginAnim, new PropertyPath(nameof(Margin)));
                storyboard.Children.Add(marginAnim);

                var widthAnim = new DoubleAnimation(Border.ActualWidth, newRect.Width / Dpi.X, duration);
                Storyboard.SetTargetProperty(widthAnim, new PropertyPath(nameof(Width)));
                storyboard.Children.Add(widthAnim);

                var heightAnim = new DoubleAnimation(Border.ActualHeight, newRect.Height / Dpi.Y, duration);
                Storyboard.SetTargetProperty(heightAnim, new PropertyPath(nameof(Height)));
                storyboard.Children.Add(heightAnim);
            }

            Border.BeginStoryboard(storyboard);
        }

        private void WindowMouseMove(object sender, MouseEventArgs e)
        {
            var point = MouseCursor.CursorPosition;

            switch (_mode)
            {
                case VideoPickerMode.Screen:
                    SelectedScreen = _screens.FirstOrDefault(screen => screen.Rectangle.Contains(point));

                    UpdateBorderAndCursor(SelectedScreen?.Rectangle);
                    break;

                case VideoPickerMode.Window:
                    SelectedWindow = _windows
                        .Where(window => Predicate?.Invoke(window) ?? true)
                        .FirstOrDefault(window => window.Rectangle.Contains(point));
                    
                    UpdateBorderAndCursor(SelectedWindow?.Rectangle);
                    break;
            }
        }

        private void WindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (_mode)
            {
                case VideoPickerMode.Screen when SelectedScreen != null:
                case VideoPickerMode.Window when SelectedWindow != null:
                    BeginClose();
                    break;
            }
        }

        public static IScreen PickScreen()
        {
            var picker = new VideoSourcePickerWindow(VideoPickerMode.Screen);

            picker.ShowDialog();

            return picker.SelectedScreen;
        }

        public static IWindow PickWindow(Predicate<IWindow> filter)
        {
            var picker = new VideoSourcePickerWindow(VideoPickerMode.Window)
            {
                Border =
                {
                    BorderThickness = new Thickness(5)
                },
                Predicate = filter
            };

            picker.ShowDialog();

            return picker.SelectedWindow;
        }
    }
}
