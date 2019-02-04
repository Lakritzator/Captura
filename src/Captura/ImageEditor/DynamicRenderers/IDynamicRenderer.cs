using System.Windows.Ink;
using System.Windows.Input;

namespace Captura.ImageEditor.DynamicRenderers
{
    public interface IDynamicRenderer
    {
        Stroke GetStroke(StylusPointCollection stylusPoints, DrawingAttributes drawingAttributes);
    }
}