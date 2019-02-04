using System;
using System.Windows.Controls;
using System.Windows.Input;
using Captura.ViewModels;

namespace Captura.Windows
{
    public partial class TrimmerWindow
    {
        public TrimmerWindow()
        {
            InitializeComponent();
            
            if (DataContext is TrimmerViewModel vm)
            {
                vm.AssignPlayer(MediaElement, this);

                Closing += (sender, e) => vm.Dispose();
            }
        }

        public void Open(string fileName)
        {
            if (DataContext is TrimmerViewModel vm)
            {
                vm.Open(fileName);
            }
        }

        private void Slider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is TrimmerViewModel vm && sender is Slider slider)
            {
                if (!vm.IsDragging)
                    return;

                vm.PlaybackPosition = TimeSpan.FromSeconds(slider.Value);

                vm.IsDragging = false;
            }
        }

        private void Slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is TrimmerViewModel vm)
            {
                vm.IsDragging = true;
            }
        }

        private void Slider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is TrimmerViewModel vm && sender is Slider slider)
            {
                vm.PlaybackPosition = TimeSpan.FromSeconds(slider.Value);
            }
        }
    }
}
