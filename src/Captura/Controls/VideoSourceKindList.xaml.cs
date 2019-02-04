using System.Windows.Controls;
using System.Windows.Input;
using Captura.Base.Video;

namespace Captura.Controls
{
    public partial class VideoSourceKindList
    {
        public VideoSourceKindList()
        {
            InitializeComponent();
        }

        private void OnVideoSourceReSelect(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.IsSelected)
            {
                if (item.DataContext is IVideoSourceProvider provider)
                {
                    provider.OnSelect();
                }
            }
        }
    }
}
