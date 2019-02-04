using System;
using System.Drawing;
using Captura.Base;
using Captura.Base.Images;

namespace Screna.ImageProviders
{
    /// <summary>
    /// Applies Overlays on an <see cref="IImageProvider"/>.
    /// </summary>
    public class OverlayedImageProvider : IImageProvider
    {
        private IOverlay[] _overlays;
        private IImageProvider _imageProvider;
        private readonly Func<Point, Point> _transform;
        
        /// <summary>
        /// Creates a new instance of <see cref="OverlayedImageProvider"/>.
        /// </summary>
        /// <param name="imageProvider">The <see cref="IImageProvider"/> to apply the Overlays on.</param>
        /// <param name="overlays">Array of <see cref="IOverlay"/>(s) to apply.</param>
        /// <param name="transform">Point Transform Function.</param>
        public OverlayedImageProvider(IImageProvider imageProvider, Func<Point, Point> transform, params IOverlay[] overlays)
        {
            _imageProvider = imageProvider ?? throw new ArgumentNullException(nameof(imageProvider));
            _overlays = overlays ?? throw new ArgumentNullException(nameof(overlays));
            _transform = transform ?? throw new ArgumentNullException(nameof(transform));

            Width = imageProvider.Width;
            Height = imageProvider.Height;
        }

        /// <inheritdoc />
        public IEditableFrame Capture()
        {
            var bmp = _imageProvider.Capture();
            
            // Overlays should have already been drawn on previous frame
            if (bmp is RepeatFrame)
            {
                return bmp;
            }
            
            foreach (var overlay in _overlays)
                overlay?.Draw(bmp, _transform);
            
            return bmp;
        }
        
        /// <inheritdoc />
        public int Height { get; }

        /// <inheritdoc />
        public int Width { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            _imageProvider.Dispose();

            foreach (var overlay in _overlays)
                overlay?.Dispose();

            _imageProvider = null;
            _overlays = null;
        }
    }
}
