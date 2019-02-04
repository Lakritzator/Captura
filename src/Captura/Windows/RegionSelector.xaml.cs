using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Captura.Base.Services;
using Captura.ViewModels;
using Color = System.Windows.Media.Color;

namespace Captura.Windows
{
    public partial class RegionSelector
    {
        private readonly IVideoSourcePicker _videoSourcePicker;
        private readonly RegionSelectorViewModel _viewModel;

        public RegionSelector(IVideoSourcePicker videoSourcePicker, RegionSelectorViewModel viewModel)
        {
            _videoSourcePicker = videoSourcePicker;
            _viewModel = viewModel;

            InitializeComponent();

            // Prevent Closing by User
            Closing += (sender, e) => e.Cancel = true;

            ModesBox.ItemsSource = new[]
            {
                new KeyValuePair<InkCanvasEditingMode, string>(InkCanvasEditingMode.None, "Pointer"),
                new KeyValuePair<InkCanvasEditingMode, string>(InkCanvasEditingMode.Ink, "Pencil"),
                new KeyValuePair<InkCanvasEditingMode, string>(InkCanvasEditingMode.EraseByPoint, "Eraser"),
                new KeyValuePair<InkCanvasEditingMode, string>(InkCanvasEditingMode.EraseByStroke, "Stroke Eraser")
            };

            ModesBox.SelectedIndex = 0;
            ColorPicker.SelectedColor = Color.FromRgb(27, 27, 27);
            SizeBox.Value = 10;

            InkCanvas.DefaultDrawingAttributes.FitToCurve = true;
        }

        private void SizeBox_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (InkCanvas != null && e.NewValue is int i)
                InkCanvas.DefaultDrawingAttributes.Height = InkCanvas.DefaultDrawingAttributes.Width = i;
        }

        private void ModesBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModesBox.SelectedValue is InkCanvasEditingMode mode)
            {
                InkCanvas.EditingMode = mode;

                if (mode == InkCanvasEditingMode.Ink)
                {
                    InkCanvas.UseCustomCursor = true;
                    InkCanvas.Cursor = Cursors.Pen;
                }
                else InkCanvas.UseCustomCursor = false;

                InkCanvas.Background = new SolidColorBrush(mode == InkCanvasEditingMode.None
                    ? Colors.Transparent
                    : Color.FromArgb(1, 0, 0, 0));
            }
        }

        private void ColorPicker_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue != null && InkCanvas != null)
                InkCanvas.DefaultDrawingAttributes.Color = e.NewValue.Value;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();

            SelectorHidden?.Invoke();
        }

        public event Action SelectorHidden;

        // Prevent Maximizing
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState != WindowState.Normal)
                WindowState = WindowState.Normal;

            base.OnStateChanged(e);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            InkCanvas.Strokes.Clear();

            base.OnRenderSizeChanged(sizeInfo);
        }

        public void Lock()
        {
            Dispatcher.Invoke(() =>
            {
                ResizeMode = ResizeMode.NoResize;
                Snapper.IsEnabled = CloseButton.IsEnabled = false;

                WidthBox.IsEnabled = HeightBox.IsEnabled = false;
            });
        }
        
        public void Release()
        {
            Dispatcher.Invoke(() =>
            {
                ResizeMode = ResizeMode.CanResize;
                Snapper.IsEnabled = CloseButton.IsEnabled = true;

                WidthBox.IsEnabled = HeightBox.IsEnabled = true;

                Show();
            });
        }

        public IntPtr Handle => new WindowInteropHelper(this).Handle;

        private void Snapper_OnClick(object sender, RoutedEventArgs e)
        {
            var win = _videoSourcePicker.PickWindow(window => window.Handle != Handle && !window.IsMaximized);

            if (win == null)
                return;

            _viewModel.SelectedRegion = win.Rectangle;

            // Prevent going outside
            if (Left < 0)
            {
                // Decrease Width
                try { Width += Left; }
                catch { }
                finally { Left = 0; }
            }

            if (Top < 0)
            {
                // Decrease Height
                try { Height += Top; }
                catch { }
                finally { Top = 0; }
            }
        }

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Thumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            void DoTop() => _viewModel.ResizeFromTop(e.VerticalChange);

            void DoLeft() => _viewModel.ResizeFromLeft(e.HorizontalChange);

            void DoBottom()
            {
                var height = Region.Height + e.VerticalChange;

                if (height > 0)
                    Region.Height = height;
            }

            void DoRight()
            {
                var width = Region.Width + e.HorizontalChange;

                if (width > 0)
                    Region.Width = width;
            }

            if (sender is FrameworkElement element)
            {
                switch (element.Tag)
                {
                    case "Bottom":
                        DoBottom();
                        break;

                    case "Left":
                        DoLeft();
                        break;

                    case "Right":
                        DoRight();
                        break;

                    case "TopLeft":
                        DoTop();
                        DoLeft();
                        break;

                    case "TopRight":
                        DoTop();
                        DoRight();
                        break;

                    case "BottomLeft":
                        DoBottom();
                        DoLeft();
                        break;

                    case "BottomRight":
                        DoBottom();
                        DoRight();
                        break;
                }
            }
        }
    }
}
