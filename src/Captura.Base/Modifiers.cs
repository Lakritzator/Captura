using System;

namespace Captura.Base
{
    [Flags]
    public enum Modifiers
    {
        None,
        Alt = 1,
        Ctrl = 2,
        Shift = 4
    }
}