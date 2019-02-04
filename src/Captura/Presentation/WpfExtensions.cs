using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using DColor = System.Drawing.Color;

namespace Captura.Presentation
{
    public static class WpfExtensions
    {
        public static void ShowAndFocus(this Window window)
        {
            if (window.IsVisible && window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }

            window.Show();

            window.Activate();
        }

        public static DColor ToDrawingColor(this Color color)
        {
            return DColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Color ToWpfColor(this DColor dColor)
        {
            return Color.FromArgb(dColor.A, dColor.R, dColor.G, dColor.B);
        }

        public static Color ParseColor(string colorString)
        {
            if (ColorConverter.ConvertFromString(colorString) is Color c)
                return c;

            return Colors.Transparent;
        }

        public static void Shake(this FrameworkElement element)
        {
            element.Dispatcher.Invoke(() =>
            {
                var transform = new TranslateTransform();
                element.RenderTransform = transform;

                const int delta = 5;

                var animation = new DoubleAnimationUsingKeyFrames
                {
                    AutoReverse = true,
                    RepeatBehavior = new RepeatBehavior(1),
                    Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                    KeyFrames =
                    {
                        new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0)),
                        new EasingDoubleKeyFrame(delta, KeyTime.FromPercent(0.25)),
                        new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0.5)),
                        new EasingDoubleKeyFrame(-delta, KeyTime.FromPercent(0.75)),
                        new EasingDoubleKeyFrame(0, KeyTime.FromPercent(1))
                    }
                };

                transform.BeginAnimation(TranslateTransform.XProperty, animation);
            });
        }

        public static bool SaveToPickedFile(this BitmapSource bitmap, string defaultFileName = null)
        {
            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ".png",
                Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp|TIFF Image|*.tiff"
            };

            if (defaultFileName != null)
            {
                sfd.FileName = Path.GetFileNameWithoutExtension(defaultFileName);

                var dir = Path.GetDirectoryName(defaultFileName);

                if (dir != null)
                {
                    sfd.InitialDirectory = dir;
                }
            }
            else sfd.FileName = "Untitled";

            if (!sfd.ShowDialog().GetValueOrDefault())
                return false;

            BitmapEncoder encoder;

            // Filter Index starts from 1
            switch (sfd.FilterIndex)
            {
                case 2:
                    encoder = new JpegBitmapEncoder();
                    break;

                case 3:
                    encoder = new BmpBitmapEncoder();
                    break;

                case 4:
                    encoder = new TiffBitmapEncoder();
                    break;

                default:
                    encoder = new PngBitmapEncoder();
                    break;
            }

            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = sfd.OpenFile())
            {
                encoder.Save(stream);
            }

            return true;
        }
    }
}
