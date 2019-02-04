using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Captura.Base.Images;
using Captura.Models;
using Captura.Presentation;
using Screna;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;

namespace Captura.Windows
{
    public partial class RegionPickerWindow
    {
        private RegionPickerWindow()
        {
            InitializeComponent();

            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            UpdateBackground();
        }

        private void UpdateBackground()
        {
            using (var bmp = ScreenShot.Capture())
            {
                var stream = new MemoryStream();
                bmp.Save(stream, ImageFormats.Png);

                stream.Seek(0, SeekOrigin.Begin);

                var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                BgImg.Source = decoder.Frames[0];
            }
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            _start = _end = null;

            Close();
        }

        private void UpdateSizeDisplay(Rect? rect)
        {
            if (rect == null)
            {
                SizeText.Visibility = Visibility.Collapsed;
            }
            else
            {
                var newRect = rect.Value;

                SizeText.Text = $"{(int) newRect.Width} x {(int) newRect.Height}";

                SizeText.Margin = new Thickness(newRect.Left + newRect.Width / 2 - SizeText.ActualWidth / 2, newRect.Top + newRect.Height / 2 - SizeText.ActualHeight / 2, 0, 0);

                SizeText.Visibility = Visibility.Visible;
            }
        }

        private void WindowMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                _end = e.GetPosition(Grid);

                var r = GetRegion();

                UpdateSizeDisplay(r);

                if (r == null)
                {
                    Border.Visibility = Visibility.Collapsed;
                    return;
                }

                var rect = r.Value;

                Border.Margin = new Thickness(rect.Left, rect.Top, 0, 0);

                Border.Width = rect.Width;
                Border.Height = rect.Height;

                Border.Visibility = Visibility.Visible;
            }
        }

        private bool _isDragging;
        private Point? _start, _end;
        private CroppingAdorner _croppingAdorner;

        private void WindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _start = e.GetPosition(Grid);
            _end = null;

            if (_croppingAdorner != null)
            {
                var layer = AdornerLayer.GetAdornerLayer(Grid);

                layer.Remove(_croppingAdorner);

                _croppingAdorner = null;
            }
        }

        private void WindowMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _end = e.GetPosition(Grid);
            Border.Visibility = Visibility.Collapsed;

            var layer = AdornerLayer.GetAdornerLayer(Grid);

            var rect = GetRegion();

            UpdateSizeDisplay(rect);

            if (rect == null)
                return;

            _croppingAdorner = new CroppingAdorner(Grid, rect.Value);

            var clr = Colors.Black;
            clr.A = 110;
            _croppingAdorner.Fill = new SolidColorBrush(clr);

            layer.Add(_croppingAdorner);

            _croppingAdorner.CropChanged += (o, args) => UpdateSizeDisplay(_croppingAdorner.SelectedRegion);

            _croppingAdorner.Checked += () =>
            {
                var r = _croppingAdorner.SelectedRegion;

                _start = r.Location;
                _end = r.BottomRight;

                Close();
            };
        }

        private Rect? GetRegion()
        {
            if (_start == null || _end == null)
            {
                return null;
            }

            var end = _end.Value;
            var start = _start.Value;

            if (end.X < start.X)
            {
                var t = start.X;
                start.X = end.X;
                end.X = t;
            }

            if (end.Y < start.Y)
            {
                var t = start.Y;
                start.Y = end.Y;
                end.Y = t;
            }

            var width = end.X - start.X;
            var height = end.Y - start.Y;

            if (width < 0.01 || height < 0.01)
            {
                return null;
            }

            return new Rect(start.X, start.Y, width, height);
        }

        private Rectangle? GetRegionScaled()
        {
            var rect = GetRegion();

            if (rect == null)
            {
                return null;
            }

            var r = rect.Value;

            return new Rectangle((int) ((Left + r.X) * Dpi.X),
                (int)((Top + r.Y) * Dpi.Y),
                (int)(r.Width * Dpi.X),
                (int)(r.Height * Dpi.Y));
        }

        public static Rectangle? PickRegion()
        {
            var picker = new RegionPickerWindow();

            picker.ShowDialog();

            return picker.GetRegionScaled();
        }
    }
}
