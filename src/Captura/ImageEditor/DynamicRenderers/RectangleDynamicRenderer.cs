using System;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;
using Captura.ImageEditor.Strokes;

namespace Captura.ImageEditor.DynamicRenderers
{
    public class RectangleDynamicRenderer : DynamicRenderer, IDynamicRenderer
    {
        private bool _isManipulating;

        private Point _firstPoint;

        public RectangleDynamicRenderer()
        {
            _firstPoint = new Point(double.NegativeInfinity, double.NegativeInfinity);
        }

        protected override void OnStylusDown(RawStylusInput rawStylusInput)
        {
            _firstPoint = rawStylusInput.GetStylusPoints().First().ToPoint();
            base.OnStylusDown(rawStylusInput);
        }

        public static void Prepare(ref Point start, ref Point end, out double width, out double height)
        {
            if (end.X < start.X)
            {
                var t = start.X;
                start.X = end.X;
                end.X = t;
            }

            if (end.Y < start.Y)
            {
                var t = start.Y;
                start.Y = end.Y;
                end.Y = t;
            }

            width = end.X - start.X;
            height = end.Y - start.Y;

            if (width <= 0)
                width = 1;

            if (height <= 0)
                height = 1;

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                width = height = Math.Min(width, height);
            }
        }

        public static void Draw(DrawingContext drawingContext, Point start, Point end, Pen pen)
        {
            Prepare(ref start, ref end, out var w, out var h);
            
            var r = new Rect(start, new Size(w, h));

            drawingContext.DrawRectangle(null, pen, r);
        }

        protected override void OnDraw(DrawingContext drawingContext, StylusPointCollection stylusPoints, Geometry geometry, Brush fillBrush)
        {
            if (!_isManipulating)
            {
                _isManipulating = true;

                var currentStylus = Stylus.CurrentStylusDevice;
                Reset(currentStylus, stylusPoints);
            }

            _isManipulating = false;

            Draw(drawingContext,
                _firstPoint,
                stylusPoints.First().ToPoint(),
                new Pen(fillBrush, 2));
        }

        protected override void OnStylusUp(RawStylusInput rawStylusInput)
        {
            _firstPoint = new Point(double.NegativeInfinity, double.NegativeInfinity);
            base.OnStylusUp(rawStylusInput);
        }

        public Stroke GetStroke(StylusPointCollection stylusPoints, DrawingAttributes drawingAttribs)
        {
            return new RectangleStroke(stylusPoints, drawingAttribs);
        }
    }
}
