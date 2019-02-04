using System.Drawing;
using Captura.Base.Settings;

namespace Captura.MouseKeyHook.Models
{
    public class MouseClickSettings : MouseOverlaySettings
    {
        public Color RightClickColor
        {
            get => Get(Color.FromArgb(3, 169, 244));
            set => Set(value);
        }

        public Color MiddleClickColor
        {
            get => Get(Color.FromArgb(76, 175, 80));
            set => Set(value);
        }
    }
}