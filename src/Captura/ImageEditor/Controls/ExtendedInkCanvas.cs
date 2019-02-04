using System.Windows.Controls;
using System.Windows.Ink;
using Captura.ImageEditor.DynamicRenderers;

namespace Captura.ImageEditor.Controls
{
    public class ExtendedInkCanvas : InkCanvas
    {
        public void SetInkTool(ExtendedInkTool inkTool)
        {
            EditingMode = inkTool.EditingMode;

            var dynamicRenderer = inkTool.DynamicRendererFunc?.Invoke();

            if (dynamicRenderer != null)
                DynamicRenderer = dynamicRenderer;

            if (inkTool.Cursor != null)
            {
                Cursor = inkTool.Cursor;
                UseCustomCursor = true;
            }
            else UseCustomCursor = false;
        }

        protected override void OnStrokeCollected(InkCanvasStrokeCollectedEventArgs inkCanvasStrokeCollectedEventArgs)
        {
            void AddCustomStroke(Stroke customStroke)
            {
                Strokes.Remove(inkCanvasStrokeCollectedEventArgs.Stroke);

                // Remove two history items
                if (DataContext is ImageEditorViewModel vm)
                {
                    vm.RemoveLastHistory();
                    vm.RemoveLastHistory();
                }

                Strokes.Add(customStroke);

                var args = new InkCanvasStrokeCollectedEventArgs(customStroke);

                base.OnStrokeCollected(args);
            }

            if (DynamicRenderer is IDynamicRenderer renderer)
            {
                AddCustomStroke(renderer.GetStroke(inkCanvasStrokeCollectedEventArgs.Stroke.StylusPoints, inkCanvasStrokeCollectedEventArgs.Stroke.DrawingAttributes));
            }
            else base.OnStrokeCollected(inkCanvasStrokeCollectedEventArgs);
        }
    }
}