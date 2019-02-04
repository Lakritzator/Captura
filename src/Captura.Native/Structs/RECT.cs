using System;
using System.Runtime.InteropServices;

namespace Captura.Native.Structs
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    // ReSharper disable once InconsistentNaming
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int dimension)
        {
            Left = Top = Right = Bottom = dimension;
        }
    }
}