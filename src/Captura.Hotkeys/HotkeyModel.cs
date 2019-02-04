using System.Windows.Forms;
using Captura.Base;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Captura.HotKeys
{
    public class HotKeyModel
    {
        public HotKeyModel(ServiceName serviceName, Keys key, Modifiers modifiers, bool isActive)
        {
            ServiceName = serviceName;
            Key = key;
            Modifiers = modifiers;
            IsActive = isActive;
        }

        // Default constructor required by Settings
        // ReSharper disable once UnusedMember.Global
        public HotKeyModel() { }

        public bool IsActive { get; set; }

        public ServiceName ServiceName { get; set; }

        public Keys Key { get; set; }

        public Modifiers Modifiers { get; set; }
    }
}
