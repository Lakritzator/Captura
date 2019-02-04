using Captura.Base.Images;
using SharpDX.DXGI;

namespace DesktopDuplication
{
    public class DesktopDuplicationImageProvider : IImageProvider
    {
        private readonly DesktopDuplicator _desktopDuplicator;

        public DesktopDuplicationImageProvider(Output1 output, bool includeCursor)
        {
            var bounds = output.Description.DesktopBounds;

            Width = bounds.Right - bounds.Left;
            Height = bounds.Bottom - bounds.Top;

            _desktopDuplicator = new DesktopDuplicator(includeCursor, output);
        }

        public int Height { get; }

        public int Width { get; }
        
        public IEditableFrame Capture()
        {
            return _desktopDuplicator.Capture();
        }

        public void Dispose()
        {
            _desktopDuplicator?.Dispose();
        }
    }
}
