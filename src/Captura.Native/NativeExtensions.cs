using System.Drawing;
using Captura.Native.Structs;

namespace Captura.Native
{
    public static class NativeExtensions
    {
        public static Rectangle ToRectangle(this RECT rect) => Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
    }
}