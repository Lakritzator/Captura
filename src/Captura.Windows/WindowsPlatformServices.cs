using System;
using System.Collections.Generic;
using Captura.Base;
using Captura.Base.Services;
using Captura.Windows.Native;
using Captura.Windows.Native.Enums;

namespace Captura.Windows
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WindowsPlatformServices : IPlatformServices
    {
        public IEnumerable<IScreen> EnumerateScreens()
        {
            return ScreenWrapper.Enumerate();
        }

        public IEnumerable<IWindow> EnumerateWindows()
        {
            return Window.EnumerateVisible();
        }

        public IWindow GetWindow(IntPtr handle)
        {
            return new Window(handle);
        }

        public IWindow DesktopWindow => Window.DesktopWindow;
        public IWindow ForegroundWindow => Window.ForegroundWindow;

        public bool DeleteFile(string filePath)
        {
            return Shell32.FileOperation(filePath, FileOperationType.Delete, 0) == 0;
        }
    }
}