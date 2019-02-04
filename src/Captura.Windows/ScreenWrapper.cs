using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Captura.Base;

namespace Captura.Windows
{
    class ScreenWrapper : IScreen
    {
        readonly Screen _screen;

        ScreenWrapper(Screen screen)
        {
            _screen = screen;
        }

        public Rectangle Rectangle => _screen.Bounds;

        public string DeviceName => _screen.DeviceName;

        public static IEnumerable<IScreen> Enumerate() => Screen.AllScreens.Select(screen => new ScreenWrapper(screen));
    }
}
