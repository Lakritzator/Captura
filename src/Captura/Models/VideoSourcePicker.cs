using System;
using System.Drawing;
using Captura.Base;
using Captura.Base.Services;
using Captura.Windows;

namespace Captura.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class VideoSourcePicker : IVideoSourcePicker
    {
        public IWindow PickWindow(Predicate<IWindow> filter = null)
        {
            return VideoSourcePickerWindow.PickWindow(filter);
        }

        public IScreen PickScreen()
        {
            return VideoSourcePickerWindow.PickScreen();
        }

        public Rectangle? PickRegion()
        {
            return RegionPickerWindow.PickRegion();
        }
    }
}