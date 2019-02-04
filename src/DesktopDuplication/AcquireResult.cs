using SharpDX;
using SharpDX.DXGI;

namespace DesktopDuplication
{
    public class AcquireResult
    {
        public AcquireResult(Result result)
        {
            Result = result;
        }

        public AcquireResult(Result result, OutputDuplicateFrameInformation frameInfo, Resource desktopResource)
        {
            Result = result;
            FrameInfo = frameInfo;
            DesktopResource = desktopResource;
        }

        public Result Result { get; }

        public OutputDuplicateFrameInformation FrameInfo { get; }

        public Resource DesktopResource { get; }
    }
}