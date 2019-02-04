using System.Drawing;

namespace Captura.Base
{
    public interface IScreen
    {
        Rectangle Rectangle { get; }

        string DeviceName { get; }
    }
}