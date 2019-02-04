using System.Drawing;

namespace Screna.Frames
{
    public class OneTimeFrame : DrawingFrameBase
    {
        public OneTimeFrame(Bitmap bitmap) : base(bitmap) { }

        public override void Dispose()
        {
            Bitmap.Dispose();
        }
    }
}