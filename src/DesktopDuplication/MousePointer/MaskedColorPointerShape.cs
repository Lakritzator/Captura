using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace DesktopDuplication.MousePointer
{
    public class MaskedColorPointerShape : MaskedPointerShape
    {
        private byte[] _maskedShapeBuffer;

        public MaskedColorPointerShape(IntPtr shapeBuffer,
            OutputDuplicatePointerShapeInformation shapeInfo,
            Direct2DEditorSession editorSession)
            : base(shapeInfo.Width, shapeInfo.Height, editorSession)
        {
            _maskedShapeBuffer = new byte[Width * Height * 4];
            Marshal.Copy(shapeBuffer, _maskedShapeBuffer, 0, _maskedShapeBuffer.Length);
        }

        protected override void OnUpdate()
        {
            for (var row = 0; row < Height; ++row)
            {
                for (var col = 0; col < Width; ++col)
                {
                    var index = row * Width * 4 + col * 4;

                    var mask = _maskedShapeBuffer[index + 3]; // Alpha value is mask

                    if (mask != 0) // 0xFF
                    {
                        // XOR with screen pixels
                        for (var i = 0; i < 3; ++i)
                        {
                            ShapeBuffer[index + i] = (byte)(DesktopBuffer[index + i] ^ _maskedShapeBuffer[index + i]);
                        }
                    }
                    else
                    {
                        // Replace screen pixels
                        for (var i = 0; i < 3; ++i)
                        {
                            ShapeBuffer[index + i] = _maskedShapeBuffer[index + i];
                        }
                    }

                    // Alpha
                    ShapeBuffer[index + 3] = 0xFF;
                }
            }
        }

        protected override void OnDispose()
        {
            _maskedShapeBuffer = null;
        }
    }
}