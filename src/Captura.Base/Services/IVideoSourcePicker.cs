using System;
using System.Drawing;

namespace Captura.Base.Services
{
    public interface IVideoSourcePicker
    {
        IWindow PickWindow(Predicate<IWindow> filter = null);

        IScreen PickScreen();

        Rectangle? PickRegion();
    }
}