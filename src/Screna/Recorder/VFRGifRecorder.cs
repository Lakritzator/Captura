using System;
using System.Threading;
using System.Threading.Tasks;
using Captura.Base;
using Captura.Base.Images;
using Screna.Gif;

namespace Screna.Recorder
{
    /// <summary>
    /// An <see cref="IRecorder"/> which records to a Gif using Delay for each frame instead of Frame Rate.
    /// </summary>
    public class VfrGifRecorder : IRecorder
    {
        #region Fields

        private readonly GifWriter _videoEncoder;
        private readonly IImageProvider _imageProvider;

        private readonly Task _recordTask;

        private readonly ManualResetEvent _stopCapturing = new ManualResetEvent(false),
            _continueCapturing = new ManualResetEvent(false);

        private readonly Timing _timing = new Timing();
        #endregion

        /// <summary>
        /// Creates a new instance of <see cref="VfrGifRecorder"/>.
        /// </summary>
        /// <param name="encoder">The <see cref="GifWriter"/> to write into.</param>
        /// <param name="imageProvider">The <see cref="IImageProvider"/> providing the individual frames.</param>
        /// <exception cref="ArgumentNullException"><paramref name="encoder"/> or <paramref name="imageProvider"/> is null.</exception>
        public VfrGifRecorder(GifWriter encoder, IImageProvider imageProvider)
        {
            // Init Fields
            _imageProvider = imageProvider ?? throw new ArgumentNullException(nameof(imageProvider));
            _videoEncoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
            
            // Not Actually Started, Waits for _continueCapturing to be Set
            _recordTask = Task.Factory.StartNew(Record);
        }

        /// <summary>
        /// Start recording.
        /// </summary>
        public void Start()
        {
            _timing.Start();
            
            _continueCapturing.Set();
        }

        private void Dispose(bool errorOccurred)
        {
            // Resume if Paused
            _continueCapturing.Set();
            
            _stopCapturing.Set();

            if (!errorOccurred)
                _recordTask.Wait();

            _continueCapturing.Dispose();
            _stopCapturing.Dispose();

            _timing.Stop();

            _imageProvider.Dispose();

            _videoEncoder.Dispose();
        }

        /// <summary>
        /// Frees resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(false);
        }

        /// <summary>
        /// Override this method with the code to pause recording.
        /// </summary>
        public void Stop()
        {
            _continueCapturing.Reset();

            _timing.Pause();
        }

        private void Record()
        {
            try
            {
                IBitmapFrame lastFrame = null;

                while (!_stopCapturing.WaitOne(0) && _continueCapturing.WaitOne())
                {
                    var frame = _imageProvider.Capture();

                    var delay = (int)_timing.Elapsed.TotalMilliseconds;

                    _timing.Stop();
                    _timing.Start();

                    // delay is the time between this and next frame
                    if (lastFrame != null)
                    {
                        _videoEncoder.WriteFrame(lastFrame, delay);
                    }

                    lastFrame = frame.GenerateFrame();
                }
            }
            catch (Exception e)
            {
                ErrorOccurred?.Invoke(e);

                Dispose(true);
            }
        }

        /// <summary>
        /// Fired when an Error occurs.
        /// </summary>
        public event Action<Exception> ErrorOccurred;
    }
}
