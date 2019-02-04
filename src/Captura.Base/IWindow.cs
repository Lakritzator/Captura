using System;
using System.Drawing;

namespace Captura.Base
{
    public interface IWindow
    {
        bool IsAlive { get; }

        bool IsVisible { get; }

        bool IsMaximized { get; }

        IntPtr Handle { get; }

        string Title { get; }

        Rectangle Rectangle { get; }
    }
}