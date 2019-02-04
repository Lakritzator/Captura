using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Services;
using Captura.Base.Settings;
using Captura.Core.Settings;
using Captura.Core.Settings.Models;
using Captura.Core.ViewModels;
using Captura.Models;
using Captura.MouseKeyHook.Models;
using Captura.Presentation;
using Captura.ViewCore.ViewModels;
using Screna;
using Screna.Overlays.Settings;
using Screna.VideoItems;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace Captura.Windows
{
    public partial class OverlayWindow
    {
        private OverlayWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            Closing += (s, e) =>
            {
                ServiceProvider.Get<Settings>().Save();
            };

            UpdateBackground();
        }

        private static OverlayWindow _instance;

        public static void ShowInstance()
        {
            if (_instance == null)
            {
                _instance = new OverlayWindow();

                _instance.Closed += (s, e) => _instance = null;
            }

            _instance.ShowAndFocus();
        }

        private void AddToGrid(Controls.LayerFrame frame, bool canResize)
        {
            Grid.Children.Add(frame);

            Panel.SetZIndex(frame, 0);

            var layer = AdornerLayer.GetAdornerLayer(frame);
            var adorner = new OverlayPositionAdorner(frame, canResize);
            layer.Add(adorner);

            adorner.PositionUpdated += frame.RaisePositionChanged;
        }

        private Controls.LayerFrame Generate(PositionedOverlaySettings settings, string text, Color backgroundColor)
        {
            var control = new Controls.LayerFrame
            {
                Border =
                {
                    Background = new SolidColorBrush(backgroundColor)
                },
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Label =
                {
                    Content = text,
                    Foreground = new SolidColorBrush(Colors.White)
                }
            };

            void Update()
            {
                int left = 0, top = 0, right = 0, bottom = 0;

                switch (settings.HorizontalAlignment)
                {
                    case Alignment.Start:
                        control.HorizontalAlignment = HorizontalAlignment.Left;
                        left = settings.X;
                        break;

                    case Alignment.Center:
                        control.HorizontalAlignment = HorizontalAlignment.Center;
                        left = settings.X;
                        break;

                    case Alignment.End:
                        control.HorizontalAlignment = HorizontalAlignment.Right;
                        right = settings.X;
                        break;
                }

                switch (settings.VerticalAlignment)
                {
                    case Alignment.Start:
                        control.VerticalAlignment = VerticalAlignment.Top;
                        top = settings.Y;
                        break;

                    case Alignment.Center:
                        control.VerticalAlignment = VerticalAlignment.Center;
                        top = settings.Y;
                        break;

                    case Alignment.End:
                        control.VerticalAlignment = VerticalAlignment.Bottom;
                        bottom = settings.Y;
                        break;
                }

                Dispatcher.Invoke(() => control.Margin = new Thickness(left, top, right, bottom));
            }

            settings.PropertyChanged += (s, e) => Update();

            Update();

            control.PositionUpdated += rect =>
            {
                settings.X = (int)rect.X;
                settings.Y = (int)rect.Y;
            };

            return control;
        }

        private Controls.LayerFrame Image(ImageOverlaySettings settings, string text)
        {
            var control = Generate(settings, text, Colors.Brown);

            control.Width = settings.ResizeWidth;
            control.Height = settings.ResizeHeight;

            control.Opacity = settings.Opacity / 100.0;

            settings.PropertyChanged += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    control.Width = settings.ResizeWidth;
                    control.Height = settings.ResizeHeight;
                    control.Opacity = settings.Opacity / 100.0;
                });
            };

            control.PositionUpdated += rect =>
            {
                settings.ResizeWidth = (int)rect.Width;
                settings.ResizeHeight = (int)rect.Height;
            };

            return control;
        }

        private Controls.LayerFrame WebCam(WebCamOverlaySettings settings)
        {
            return Image(settings, "WebCam");
        }

        private Controls.LayerFrame Text(TextOverlaySettings settings, string text)
        {
            var control = Generate(settings, text, ConvertColor(settings.BackgroundColor));
            
            control.Label.FontSize = settings.FontSize;

            control.Border.Padding = new Thickness(settings.HorizontalPadding,
                settings.VerticalPadding,
                settings.HorizontalPadding,
                settings.VerticalPadding);

            control.Label.Foreground = new SolidColorBrush(ConvertColor(settings.FontColor));
            control.Border.BorderThickness = new Thickness(settings.BorderThickness);
            control.Border.BorderBrush = new SolidColorBrush(ConvertColor(settings.BorderColor));

            control.Border.CornerRadius = new CornerRadius(settings.CornerRadius);

            settings.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(settings.BackgroundColor):
                        control.Border.Background = new SolidColorBrush(ConvertColor(settings.BackgroundColor));
                        break;

                    case nameof(settings.FontColor):
                        control.Label.Foreground = new SolidColorBrush(ConvertColor(settings.FontColor));
                        break;

                    case nameof(settings.BorderThickness):
                        control.Border.BorderThickness = new Thickness(settings.BorderThickness);
                        break;

                    case nameof(settings.BorderColor):
                        control.Border.BorderBrush = new SolidColorBrush(ConvertColor(settings.BorderColor));
                        break;

                    case nameof(settings.FontSize):
                        control.Label.FontSize = settings.FontSize;
                        break;

                    case nameof(settings.HorizontalPadding):
                    case nameof(settings.VerticalPadding):
                        control.Border.Padding = new Thickness(settings.HorizontalPadding,
                            settings.VerticalPadding,
                            settings.HorizontalPadding,
                            settings.VerticalPadding);
                        break;

                    case nameof(settings.CornerRadius):
                        control.Border.CornerRadius = new CornerRadius(settings.CornerRadius);
                        break;
                }
            };

            return control;
        }

        private Controls.LayerFrame Censor(CensorOverlaySettings settings)
        {
            var control = Generate(settings, "Censored", Colors.Black);

            control.Width = settings.Width;
            control.Height = settings.Height;

            settings.PropertyChanged += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    control.Width = settings.Width;
                    control.Height = settings.Height;
                });
            };

            control.PositionUpdated += rect =>
            {
                settings.Width = (int)rect.Width;
                settings.Height = (int)rect.Height;
            };

            return control;
        }

        private static Color ConvertColor(System.Drawing.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private Controls.LayerFrame Keystrokes(KeystrokesSettings settings)
        {
            var control = Text(settings, "Keystrokes");

            void SetVisibility()
            {
                control.Visibility = settings.SeparateTextFile ? Visibility.Collapsed : Visibility;
            }

            SetVisibility();

            settings.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(settings.SeparateTextFile):
                        SetVisibility();
                        break;
                }
            };

            return control;
        }

        private readonly List<Controls.LayerFrame> _textOverlays = new List<Controls.LayerFrame>();
        private readonly List<Controls.LayerFrame> _imageOverlays = new List<Controls.LayerFrame>();
        private readonly List<Controls.LayerFrame> _censorOverlays = new List<Controls.LayerFrame>();

        private void UpdateCensorOverlays(IEnumerable<CensorOverlaySettings> settings)
        {
            foreach (var overlay in _censorOverlays)
            {
                Grid.Children.Remove(overlay);
            }

            _censorOverlays.Clear();

            foreach (var setting in settings)
            {
                var control = Censor(setting);
                control.Visibility = setting.Display ? Visibility.Visible : Visibility.Collapsed;

                setting.PropertyChanged += (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(setting.Display):
                            control.Visibility = setting.Display ? Visibility.Visible : Visibility.Collapsed;
                            break;
                    }
                };

                _censorOverlays.Add(control);
            }

            foreach (var overlay in _censorOverlays)
            {
                AddToGrid(overlay, true);

                Panel.SetZIndex(overlay, -1);
            }
        }

        private void UpdateTextOverlays(IEnumerable<CustomOverlaySettings> settings)
        {
            foreach (var overlay in _textOverlays)
            {
                Grid.Children.Remove(overlay);
            }

            _textOverlays.Clear();

            foreach (var setting in settings)
            {
                var control = Text(setting, setting.Text);
                control.Visibility = setting.Display ? Visibility.Visible : Visibility.Collapsed;

                setting.PropertyChanged += (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(setting.Text):
                            control.Label.Content = setting.Text;
                            break;

                        case nameof(setting.Display):
                            control.Visibility = setting.Display ? Visibility.Visible : Visibility.Collapsed;
                            break;
                    }
                };

                _textOverlays.Add(control);
            }

            foreach (var overlay in _textOverlays)
            {
                AddToGrid(overlay, false);

                Panel.SetZIndex(overlay, 1);
            }
        }

        private void UpdateImageOverlays(IEnumerable<CustomImageOverlaySettings> settings)
        {
            foreach (var overlay in _imageOverlays)
            {
                Grid.Children.Remove(overlay);
            }

            _imageOverlays.Clear();

            foreach (var setting in settings)
            {
                var control = Image(setting, setting.Source);
                control.Visibility = setting.Display ? Visibility.Visible : Visibility.Collapsed;

                setting.PropertyChanged += (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(setting.Source):
                            control.Label.Content = setting.Source;
                            break;

                        case nameof(setting.Display):
                            control.Visibility = setting.Display ? Visibility.Visible : Visibility.Collapsed;
                            break;
                    }
                };

                _imageOverlays.Add(control);
            }

            foreach (var overlay in _imageOverlays)
            {
                AddToGrid(overlay, true);

                Panel.SetZIndex(overlay, 2);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            PlaceOverlays();
        }

        private async void UpdateBackground()
        {
            var vm = ServiceProvider.Get<VideoSourcesViewModel>();

            IBitmapImage bmp;

            switch (vm.SelectedVideoSourceKind?.Source)
            {
                case FullScreenItem _:
                case NoVideoItem _:
                    bmp = ScreenShot.Capture();
                    break;

                default:
                    var screenShotModel = ServiceProvider.Get<ScreenShotModel>();
                    bmp = await screenShotModel.GetScreenShot();
                    break;
            }

            using (bmp)
            {
                var stream = new MemoryStream();
                bmp.Save(stream, ImageFormats.Png);

                stream.Seek(0, SeekOrigin.Begin);

                var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                Img.Source = decoder.Frames[0];
            }
        }

        private void UpdateScale()
        {
            if (Img.Source == null)
                return;

            var scaleX = Img.ActualWidth / Img.Source.Width;
            var scaleY = Img.ActualHeight / Img.Source.Height;

            Scale.ScaleX = scaleX / Dpi.X;
            Scale.ScaleY = scaleY / Dpi.Y;
        }

        private void PlaceOverlays()
        {
            var settings = ServiceProvider.Get<Settings>();

            var censorOverlayVm = ServiceProvider.Get<CensorOverlaysViewModel>();

            UpdateCensorOverlays(censorOverlayVm.Collection);
            (censorOverlayVm.Collection as INotifyCollectionChanged).CollectionChanged += (s, e) => UpdateCensorOverlays(censorOverlayVm.Collection);

            PrepareMousePointer(settings.MousePointerOverlay);
            PrepareMouseClick(settings.Clicks);

            var webcam = WebCam(settings.WebCamOverlay);
            AddToGrid(webcam, true);

            var keystrokes = Keystrokes(settings.Keystrokes);
            AddToGrid(keystrokes, false);

            var elapsed = Text(settings.Elapsed, "00:00:00");
            AddToGrid(elapsed, false);

            var textOverlayVm = ServiceProvider.Get<CustomOverlaysViewModel>();

            UpdateTextOverlays(textOverlayVm.Collection);
            (textOverlayVm.Collection as INotifyCollectionChanged).CollectionChanged += (s, e) => UpdateTextOverlays(textOverlayVm.Collection);

            var imgOverlayVm = ServiceProvider.Get<CustomImageOverlaysViewModel>();

            UpdateImageOverlays(imgOverlayVm.Collection);
            (imgOverlayVm.Collection as INotifyCollectionChanged).CollectionChanged += (s, e) => UpdateImageOverlays(imgOverlayVm.Collection);
        }

        private void PrepareMouseClick(MouseClickSettings settings)
        {
            void Update()
            {
                var d = (settings.Radius + settings.BorderThickness) * 2;

                MouseClick.Width = MouseClick.Height = d;
                MouseClick.StrokeThickness = settings.BorderThickness;
                MouseClick.Stroke = new SolidColorBrush(ConvertColor(settings.BorderColor));
            }

            Update();
            
            settings.PropertyChanged += (s, e) => Dispatcher.Invoke(Update);
        }

        private void PrepareMousePointer(MouseOverlaySettings settings)
        {
            void Update()
            {
                var d = (settings.Radius + settings.BorderThickness) * 2;

                MousePointer.Width = MousePointer.Height = d;
                MousePointer.StrokeThickness = settings.BorderThickness;
                MousePointer.Stroke = new SolidColorBrush(ConvertColor(settings.BorderColor));
                MousePointer.Fill = new SolidColorBrush(ConvertColor(settings.Color));
            }

            Update();

            settings.PropertyChanged += (s, e) => Dispatcher.Invoke(Update);
        }

        private void OverlayWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScale();
        }

        private void Img_OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateScale();
        }

        private static Color GetClickColor(MouseButton button)
        {
            var settings = ServiceProvider.Get<Settings>();

            switch (button)
            {
                case MouseButton.Middle:
                    return ConvertColor(settings.Clicks.MiddleClickColor);

                case MouseButton.Right:
                    return ConvertColor(settings.Clicks.RightClickColor);
                    
                default:
                    return ConvertColor(settings.Clicks.Color);
            }
        }

        private bool _dragging;

        private void UpdateMouseClickPosition(Point position)
        {
            MouseClick.Margin = new Thickness(position.X - MouseClick.ActualWidth / 2, position.Y - MouseClick.ActualHeight / 2, 0, 0);
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragging = true;

            UpdateMouseClickPosition(e.GetPosition(Grid));

            MouseClick.Fill = new SolidColorBrush(GetClickColor(e.ChangedButton));

            MouseClick.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new Duration(TimeSpan.FromMilliseconds(200))));
        }

        private void MouseClickEnd()
        {
            MouseClick.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(300))));

            _dragging = false;
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseClickEnd();
        }

        private bool IsOutsideGrid(Point point)
        {
            return point.X <= 0 || point.Y <= 0
                   || point.X + MouseClick.ActualWidth / 2 >= Grid.ActualWidth
                   || point.Y + MouseClick.ActualHeight / 2 >= Grid.ActualHeight;
        }

        private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (ServiceProvider.Get<Settings>().MousePointerOverlay.Display)
                MousePointer.Visibility = Visibility.Visible;

            var position = e.GetPosition(Grid);

            if (IsOutsideGrid(position))
            {
                MousePointer.Visibility = Visibility.Collapsed;

                return;
            }

            if (_dragging)
            {
                UpdateMouseClickPosition(position);
            }

            position.X -= MouseClick.ActualWidth / 2;
            position.Y -= MouseClick.ActualHeight / 2;

            MousePointer.Margin = new Thickness(position.X, position.Y, 0, 0);
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            MouseClickEnd();

            MousePointer.Visibility = Visibility.Collapsed;
        }
    }
}
