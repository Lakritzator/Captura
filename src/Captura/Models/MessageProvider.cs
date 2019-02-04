using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Captura.Base.Audio;
using Captura.Base.Services;
using Captura.FFmpeg;
using Captura.Loc;
using Captura.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using ExceptionWindow = Captura.Windows.ExceptionWindow;

namespace Captura.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MessageProvider : IMessageProvider
    {
        private readonly IAudioPlayer _audioPlayer;

        public MessageProvider(IAudioPlayer audioPlayer)
        {
            _audioPlayer = audioPlayer;
        }

        public void ShowError(string message, string header = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new ModernDialog
                {
                    Title = LanguageManager.Instance.ErrorOccurred,
                    Content = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text = header,
                                Margin = new Thickness(0, 0, 0, 10),
                                FontSize = 15
                            },

                            new ScrollViewer
                            {
                                Content = message,
                                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                                Padding = new Thickness(0, 0, 0, 10)
                            }
                        }
                    }
                };

                dialog.OkButton.Content = LanguageManager.Instance.Ok;
                dialog.Buttons = new[] { dialog.OkButton };

                dialog.BackgroundContent = new Grid
                {
                    Background = new SolidColorBrush(Color.FromArgb(255, 244, 67, 54)),
                    VerticalAlignment = VerticalAlignment.Top,
                    Height = 10
                };

                _audioPlayer.Play(SoundKind.Error);

                dialog.ShowDialog();
            });
        }

        public void ShowFFmpegUnavailable()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new ModernDialog
                {
                    Title = "FFmpeg Unavailable",
                    Content = "FFmpeg was not found on your system.\n\nSelect FFmpeg Folder if you alrady have FFmpeg on your system, else Download FFmpeg."
                };

                // Yes -> Select FFmpeg Folder
                dialog.YesButton.Content = LanguageManager.Instance.SelectFFmpegFolder;
                dialog.YesButton.Click += (sender, e) => FFmpegService.SelectFFmpegFolder();

                // No -> Download FFmpeg
                dialog.NoButton.Content = "Download FFmpeg";
                dialog.NoButton.Click += (sender, e) => FFmpegService.FFmpegDownloader?.Invoke();

                dialog.CancelButton.Content = "Cancel";

                dialog.Buttons = new[] { dialog.YesButton, dialog.NoButton, dialog.CancelButton };

                _audioPlayer.Play(SoundKind.Error);

                dialog.ShowDialog();
            });
        }

        public void ShowException(Exception exception, string message, bool blocking = false)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var win = new ExceptionWindow(exception, message);

                _audioPlayer.Play(SoundKind.Error);

                if (blocking)
                {
                    win.ShowDialog();
                }
                else win.ShowAndFocus();
            });
        }

        public bool ShowYesNo(string message, string title)
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new ModernDialog
                {
                    Title = title,
                    Content = new ScrollViewer
                    {
                        Content = message,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Padding = new Thickness(0, 0, 0, 10)
                    }
                };

                var result = false;

                dialog.YesButton.Content = LanguageManager.Instance.Yes;
                dialog.YesButton.Click += (sender, e) => result = true;

                dialog.NoButton.Content = LanguageManager.Instance.No;

                dialog.Buttons = new[] { dialog.YesButton, dialog.NoButton };

                dialog.ShowDialog();

                return result;
            });
        }
    }
}
