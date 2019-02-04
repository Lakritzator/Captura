using System;
using System.Runtime.InteropServices;
using Captura.Native.Structs;

namespace Captura.Windows.Native
{
    static class DwmApi
    {
        const string DllName = "dwmapi.dll";

        [DllImport(DllName)]
        public static extern int DwmGetWindowAttribute(IntPtr window, int attribute, out bool value, int size);

        [DllImport(DllName)]
        public static extern int DwmGetWindowAttribute(IntPtr window, int attribute, ref RECT value, int size);
    }
}