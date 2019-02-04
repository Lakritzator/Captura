using System;
using System.Drawing;

namespace Captura.Base.Images
{
    public class RepeatFrame : IBitmapFrame, IEditableFrame
    {
        RepeatFrame() { }

        public static RepeatFrame Instance { get; } = new RepeatFrame();

        IBitmapFrame IEditableFrame.GenerateFrame() => Instance;

        int IBitmapFrame.Width { get; } = -1;

        float IEditableFrame.Height { get; } = -1;

        IDisposable IBitmapLoader.CreateBitmapBgr32(Size size, IntPtr memoryData, int stride)
        {
            throw new NotImplementedException();
        }

        IDisposable IBitmapLoader.LoadBitmap(string fileName, out Size size)
        {
            throw new NotImplementedException();
        }

        float IEditableFrame.Width { get; } = -1;

        int IBitmapFrame.Height { get; } = -1;

        void IDisposable.Dispose() { }

        void IEditableFrame.DrawImage(object image, Rectangle? region, int opacity)
        {
            throw new NotImplementedException();
        }

        void IEditableFrame.FillRectangle(Color color, RectangleF rectangle)
        {
            throw new NotImplementedException();
        }

        void IEditableFrame.FillRectangle(Color color, RectangleF rectangle, int cornerRadius)
        {
            throw new NotImplementedException();
        }

        void IEditableFrame.DrawRectangle(Color color, float strokeWidth, RectangleF rectangle)
        {
            throw new NotImplementedException();
        }

        void IEditableFrame.DrawRectangle(Color color, float strokeWidth, RectangleF rectangle, int cornerRadius)
        {
            throw new NotImplementedException();
        }

        void IEditableFrame.FillEllipse(Color color, RectangleF rectangle)
        {
            throw new NotImplementedException();
        }

        void IEditableFrame.DrawEllipse(Color color, float strokeWidth, RectangleF rectangle)
        {
            throw new NotImplementedException();
        }

        SizeF IEditableFrame.MeasureString(string text, int fontSize)
        {
            throw new NotImplementedException();
        }

        void IEditableFrame.DrawString(string text, int fontSize, Color color, RectangleF layoutRectangle)
        {
            throw new NotImplementedException();
        }

        void IBitmapFrame.CopyTo(byte[] buffer, int length)
        {
            throw new NotImplementedException();
        }
    }
}