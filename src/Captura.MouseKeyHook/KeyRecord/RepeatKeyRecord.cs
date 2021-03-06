using System;

namespace Captura.MouseKeyHook.KeyRecord
{
    class RepeatKeyRecord : IKeyRecord
    {
        public RepeatKeyRecord(KeyRecord repeated)
        {
            Repeated = repeated;

            Increment();
        }

        public bool Control => Repeated.Control;
        public bool Shift => Repeated.Shift;
        public bool Alt => Repeated.Alt;

        public DateTime TimeStamp { get; private set; }

        public KeyRecord Repeated { get; }

        public int Repeat { get; private set; } = 1;

        public void Increment()
        {
            ++Repeat;

            TimeStamp = DateTime.Now;
        }

        public string Display => $"{Repeated} x {Repeat}";
    }
}