using System;
using System.Windows.Interop;

namespace Captura.Models
{
    public class HotKeyListener
    {
        private const int WindowsMessageHotKey = 786;

        public HotKeyListener()
        {
            ComponentDispatcher.ThreadPreprocessMessage += (ref MSG message, ref bool handled) =>
            {
                if (message.message == WindowsMessageHotKey)
                {
                    var id = message.wParam.ToInt32();

                    HotKeyReceived?.Invoke(id);
                }
            };
        }

        public event Action<int> HotKeyReceived;
    }
}