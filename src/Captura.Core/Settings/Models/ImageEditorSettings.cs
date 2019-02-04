using System.Drawing;
using Captura.Base.Settings;

namespace Captura.Core.Settings.Models
{
    public class ImageEditorSettings : PropertyStore
    {
        public Color BrushColor
        {
            get => Get(Color.FromArgb(27, 27, 27));
            set => Set(value);
        }

        public int BrushSize
        {
            get => Get(2);
            set => Set(value);
        }
    }
}