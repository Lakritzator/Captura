using System;
using System.Collections.Generic;

namespace Captura.Base.Services
{
    public interface IPlatformServices
    {
        IEnumerable<IScreen> EnumerateScreens();

        IEnumerable<IWindow> EnumerateWindows();

        IWindow GetWindow(IntPtr handle);

        IWindow DesktopWindow { get; }

        IWindow ForegroundWindow { get; }

        bool DeleteFile(string filePath);
    }
}