using System;

namespace Captura.MouseKeyHook.KeyRecord
{
    interface IKeyRecord
    {
        DateTime TimeStamp { get; }

        string Display { get; }

        bool Control { get; }
        bool Shift { get; }
        bool Alt { get; }
    }
}