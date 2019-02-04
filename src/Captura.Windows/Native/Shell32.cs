using System;
using System.Runtime.InteropServices;
using Captura.Windows.Native.Enums;
using Captura.Windows.Native.Structs;

namespace Captura.Windows.Native
{
    static class Shell32
    {
        const string DllName = "shell32.dll";

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        public static extern int SHFileOperation(ref ShFileOpStruct fileOp);

        public static int FileOperation(string path, FileOperationType operationType, FileOperationFlags flags)
        {
            try
            {
                var fs = new ShFileOpStruct
                {
                    Func = operationType,
                    From = path + '\0' + '\0',
                    Flags = flags
                };

                return SHFileOperation(ref fs);
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}