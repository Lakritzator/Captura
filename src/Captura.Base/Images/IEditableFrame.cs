using System.Drawing;

namespace Captura.Base.Images
{
    public interface IEditableFrame : IBitmapLoader
    {
        float Width { get; }
        float Height { get; }

        void DrawImage(object image, Rectangle? region, int opacity = 100);

        void FillRectangle(Color color, RectangleF rectangle);

        void FillRectangle(Color color, RectangleF rectangle, int cornerRadius);

        void DrawRectangle(Color color, float strokeWidth, RectangleF rectangle);

        void DrawRectangle(Color color, float strokeWidth, RectangleF rectangle, int cornerRadius);

        void FillEllipse(Color color, RectangleF rectangle);

        void DrawEllipse(Color color, float strokeWidth, RectangleF rectangle);

        SizeF MeasureString(string text, int fontSize);

        void DrawString(string text, int fontSize, Color color, RectangleF layoutRectangle);

        IBitmapFrame GenerateFrame();
    }
}