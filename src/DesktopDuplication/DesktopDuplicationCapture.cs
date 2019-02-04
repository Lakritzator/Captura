using System;
using System.Threading.Tasks;
using DesktopDuplication.MousePointer;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using ResultCode = SharpDX.DXGI.ResultCode;

namespace DesktopDuplication
{
    public class DesktopDuplicationCapture : IDisposable
    {
        private readonly Output1 _output;
        private readonly Device _device;
        private OutputDuplication _outputDuplication;

        private readonly object _syncLock = new object();

        public DesktopDuplicationCapture(Device device, Output1 output)
        {
            _device = device;
            _output = output;

            Init();
        }

        public void Dispose()
        {
            try
            {
                _acquireTask?.Wait();
            }
            catch
            {
                // Ignore in dispose
            }

            _outputDuplication?.Dispose();
        }

        public void Init()
        {
            lock (_syncLock)
            {
                _acquireTask?.Wait();

                _acquireTask = null;
                _outputDuplication?.Dispose();

                try
                {
                    _outputDuplication = _output.DuplicateOutput(_device);
                }
                catch (SharpDXException e) when (e.Descriptor == ResultCode.NotCurrentlyAvailable)
                {
                    throw new Exception("There is already the maximum number of applications using the Desktop Duplication API running, please close one of the applications and try again.", e);
                }
                catch (SharpDXException e) when (e.Descriptor == ResultCode.Unsupported)
                {
                    throw new NotSupportedException("Desktop Duplication is not supported on this system.\nIf you have multiple graphic cards, try running Captura on integrated graphics.", e);
                }
            }
        }

        private Task<AcquireResult> _acquireTask;

        private void BeginAcquireFrame()
        {
            const int timeout = 5000;

            _acquireTask = Task.Run(() =>
            {
                try
                {
                    var result = _outputDuplication.TryAcquireNextFrame(timeout, out var frameInfo, out var desktopResource);

                    return new AcquireResult(result, frameInfo, desktopResource);
                }
                catch
                {
                    return new AcquireResult(Result.Fail);
                }
            });
        }

        public bool Get(Texture2D texture, DxMousePointer dxMousePointer)
        {
            lock (_syncLock)
            {
                if (_acquireTask == null)
                {
                    BeginAcquireFrame();

                    return false;
                }

                var acquireResult = _acquireTask.Result;

                if (acquireResult.Result == ResultCode.WaitTimeout)
                {
                    BeginAcquireFrame();

                    return false;
                }

                if (acquireResult.Result.Failure)
                {
                    throw new Exception($"Failed to acquire next frame: {acquireResult.Result.Code}");
                }

                using (acquireResult.DesktopResource)
                using (var tempTexture = acquireResult.DesktopResource.QueryInterface<Texture2D>())
                {
                    dxMousePointer?.Update(tempTexture, acquireResult.FrameInfo, _outputDuplication);

                    _device.ImmediateContext.CopyResource(tempTexture, texture);
                }

                _outputDuplication.ReleaseFrame();

                BeginAcquireFrame();

                return true;
            }
        }
    }
}