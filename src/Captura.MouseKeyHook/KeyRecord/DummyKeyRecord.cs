using System;

namespace Captura.MouseKeyHook.KeyRecord
{
    class DummyKeyRecord : IKeyRecord
    {
        public DummyKeyRecord(string display)
        {
            Display = display;

            TimeStamp = DateTime.Now;
        }

        public bool Control => false;
        public bool Shift => false;
        public bool Alt => false;

        public DateTime TimeStamp { get; }

        public string Display { get; }
    }
}