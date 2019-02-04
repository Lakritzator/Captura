using System;
using System.Linq;
using System.Windows.Forms;
using Captura.Base;
using Captura.Native;

namespace Captura.HotKeys
{
    public class HotKey : NotifyPropertyChanged
    {
        private Service _service;

        public Service Service
        {
            get => _service;
            set
            {
                _service = value;
                
                OnPropertyChanged();
            }
        }

        public HotKey(HotKeyModel model)
        {
            Service = HotKeyManager.AllServices.FirstOrDefault(service => service.ServiceName == model.ServiceName);
            Key = model.Key;
            Modifiers = model.Modifiers;

            IsActive = model.IsActive;
        }

        private bool _active;

        public bool IsActive
        {
            get => _active;
            set
            {
                _active = value;

                if (value && !IsRegistered)
                {
                    Register();
                }
                else if (!value && IsRegistered)
                {
                    UnRegister();
                }

                OnPropertyChanged();
            }
        }

        public bool IsRegistered { get; private set; }

        public ushort Id { get; private set; }

        public void Register()
        {
            if (IsRegistered || Key == Keys.None)
                return;

            // Generate Unique ID
            var uid = Guid.NewGuid().ToString("N");
            Id = Kernel32.GlobalAddAtom(uid);
            
            if (User32.RegisterHotKey(IntPtr.Zero, Id, (int) Modifiers, (uint) Key))
                IsRegistered = true;
            else
            {
                Kernel32.GlobalDeleteAtom(Id);
                Id = 0;
            }
        }
        
        public Keys Key { get; private set; }

        public Modifiers Modifiers { get; private set; }

        public void Change(Keys newKey, Modifiers newModifiers)
        {
            UnRegister();

            Key = newKey;
            Modifiers = newModifiers;

            Register();
        }

        public void UnRegister()
        {
            if (!IsRegistered)
                return;

            if (!User32.UnregisterHotKey(IntPtr.Zero, Id))
            {
                return;
            }

            IsRegistered = false;

            Kernel32.GlobalDeleteAtom(Id);
            Id = 0;
        }

        public override string ToString()
        {
            var text = "";

            if (Modifiers.HasFlag(Modifiers.Ctrl))
                text += "Ctrl + ";

            if (Modifiers.HasFlag(Modifiers.Alt))
                text += "Alt + ";

            if (Modifiers.HasFlag(Modifiers.Shift))
                text += "Shift + ";

            // Handle Number keys
            if (Key >= Keys.D0 && Key <= Keys.D9)
            {
                text += Key - Keys.D0;
            }
            else if (Key >= Keys.NumPad0 && Key <= Keys.NumPad9)
            {
                text += Key - Keys.NumPad0;
            }
            else text += Key;

            return text;
        }
    }
}