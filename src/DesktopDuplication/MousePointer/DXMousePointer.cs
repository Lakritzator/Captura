using System;
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Rectangle = System.Drawing.Rectangle;

namespace DesktopDuplication.MousePointer
{
    public class DxMousePointer : IDisposable
    {
        private readonly Direct2DEditorSession _editorSession;

        private IntPtr _ptrShapeBuffer;
        private int _ptrShapeBufferSize;
        private OutputDuplicatePointerShapeInformation _ptrShapeInfo;
        private OutputDuplicatePointerPosition _pointerPosition;
        private IPointerShape _pointerShape;

        private const int PtrShapeMonochrome = 1;
        private const int PtrShapeColor = 2;
        private const int PtrShapeMaskedColor = 4;

        public DxMousePointer(Direct2DEditorSession editorSession)
        {
            _editorSession = editorSession;
        }

        public void Update(Texture2D desktopTexture, OutputDuplicateFrameInformation outputDuplicateFrameInformation, OutputDuplication outputDuplication)
        {
            // No update
            if (outputDuplicateFrameInformation.LastMouseUpdateTime == 0)
            {
                return;
            }

            _pointerPosition = outputDuplicateFrameInformation.PointerPosition;

            if (outputDuplicateFrameInformation.PointerShapeBufferSize != 0)
            {
                if (outputDuplicateFrameInformation.PointerShapeBufferSize > _ptrShapeBufferSize)
                {
                    _ptrShapeBufferSize = outputDuplicateFrameInformation.PointerShapeBufferSize;

                    _ptrShapeBuffer = _ptrShapeBuffer != IntPtr.Zero
                        ? Marshal.ReAllocCoTaskMem(_ptrShapeBuffer, _ptrShapeBufferSize)
                        : Marshal.AllocCoTaskMem(_ptrShapeBufferSize);
                }

                outputDuplication.GetFramePointerShape(_ptrShapeBufferSize,
                    _ptrShapeBuffer,
                    out _,
                    out _ptrShapeInfo);

                _pointerShape?.Dispose();

                switch (_ptrShapeInfo.Type)
                {
                    case PtrShapeMonochrome:
                        _pointerShape = new MonochromePointerShape(_ptrShapeBuffer,
                            _ptrShapeInfo,
                            _editorSession);
                        break;

                    case PtrShapeMaskedColor:
                        _pointerShape = new MaskedColorPointerShape(_ptrShapeBuffer,
                            _ptrShapeInfo,
                            _editorSession);
                        break;

                    case PtrShapeColor:
                        _pointerShape = new ColorPointerShape(_ptrShapeBuffer,
                            _ptrShapeInfo,
                            _editorSession.RenderTarget);
                        break;
                }
            }

            _pointerShape?.Update(desktopTexture, _pointerPosition);
        }

        public void Draw(Direct2DEditor editor)
        {
            if (!_pointerPosition.Visible)
            {
                return;
            }

            var bitmap = _pointerShape?.GetBitmap();

            if (bitmap == null)
                return;

            var rect = new Rectangle(_pointerPosition.Position.X,
                _pointerPosition.Position.Y,
                (int) bitmap.Size.Width,
                (int) bitmap.Size.Height);

            editor.DrawImage(bitmap, rect);
        }

        public void Dispose()
        {
            if (_ptrShapeBuffer == IntPtr.Zero)
            {
                return;
            }

            _pointerShape?.Dispose();
            Marshal.FreeCoTaskMem(_ptrShapeBuffer);
        }
    }
}