using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Captura.Base.Services;
using Captura.ImageEditor;
using Captura.Models;

namespace Captura.Controls
{
    public partial class ScreenShotBalloon : IRemoveRequester
    {
        private readonly string _filePath;

        public ScreenShotBalloon(string filePath)
        {
            _filePath = filePath;
            DataContext = Path.GetFileName(filePath);

            InitializeComponent();

            // Do not assign image directly, cache it, else the file can't be deleted.
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(filePath);
            image.EndInit();
            Img.Source = image;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => OnClose();

        private void OnClose()
        {
            RemoveRequested?.Invoke();
        }

        public event Action RemoveRequested;

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ServiceProvider.LaunchFile(new ProcessStartInfo(_filePath));

            OnClose();
        }

        private void EditButton_OnClick(object sender, RoutedEventArgs e)
        {
            var win = new ImageEditorWindow();
            win.Open(_filePath);

            win.Show();
        }
    }
}