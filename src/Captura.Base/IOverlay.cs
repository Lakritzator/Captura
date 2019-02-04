using System;
using System.Drawing;
using Captura.Base.Images;

namespace Captura.Base
{
    /// <summary>
    /// Draws over a Capured image.
    /// </summary>
    public interface IOverlay : IDisposable
    {
        /// <summary>
        /// Draws the Overlay.
        /// </summary>
        void Draw(IEditableFrame editor, Func<Point, Point> pointTransform = null);
    }
}
