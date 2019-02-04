using System;
using System.Windows;
using Captura.Base.Services;
using Captura.ImageEditor;
using Captura.Presentation;
using Captura.Windows;

namespace Captura.Models
{
    internal class MainWindowProvider : IMainWindow
    {
        private readonly Func<Window> _window;

        public MainWindowProvider(Func<Window> window)
        {
            _window = window;
        }

        public bool IsVisible
        {
            get => _window.Invoke().IsVisible;
            set
            {
                if (value)
                    _window.Invoke().Show();
                else _window.Invoke().Hide();
            }
        }

        public bool IsMinimized
        {
            get => _window.Invoke().WindowState == WindowState.Minimized;
            set => _window.Invoke().WindowState = value ? WindowState.Minimized : WindowState.Normal;
        }

        public void EditImage(string fileName)
        {
            var win = new ImageEditorWindow();

            win.Open(fileName);

            win.ShowAndFocus();
        }

        public void CropImage(string fileName)
        {
            new Windows.CropWindow(fileName).ShowAndFocus();
        }

        public void TrimMedia(string fileName)
        {
            var win = new TrimmerWindow();

            win.Open(fileName);

            win.ShowAndFocus();
        }

        public void UploadToYouTube(string fileName)
        {
            var win = new Windows.YouTubeUploaderWindow();

            win.Open(fileName);

            win.ShowAndFocus();
        }
    }
}
