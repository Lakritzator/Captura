using System;
using System.Runtime.InteropServices;
using Captura.Windows.Native.Enums;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Captura.Windows.Native.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    struct ShFileOpStruct
    {
        public IntPtr hwnd;

        public FileOperationType Func;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string From;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string To;

        public FileOperationFlags Flags;

        [MarshalAs(UnmanagedType.Bool)]
        public bool AnyOperationsAborted;

        public IntPtr NameMappings;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string ProgressTitle;
    }
}