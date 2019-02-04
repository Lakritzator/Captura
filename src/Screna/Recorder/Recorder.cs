using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Captura.Base;
using Captura.Base.Audio;
using Captura.Base.Images;
using Captura.Base.Video;

// ReSharper disable MethodSupportsCancellation

namespace Screna.Recorder
{
    /// <summary>
    /// Default implementation of <see cref="IRecorder"/> interface.
    /// Can output to <see cref="IVideoFileWriter"/> or <see cref="IAudioFileWriter"/>.
    /// </summary>
    public class Recorder : IRecorder
    {
        #region Fields

        private IAudioProvider _audioProvider;
        private IVideoFileWriter _videoWriter;
        private IAudioFileWriter _audioWriter;
        private IImageProvider _imageProvider;

        private readonly int _frameRate;

        private readonly Stopwatch _sw;

        private readonly ManualResetEvent _continueCapturing;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        private readonly Task _recordTask;

        private readonly object _syncLock = new object();
        #endregion

        /// <summary>
        /// Creates a new instance of <see cref="IRecorder"/> writing to <see cref="IVideoFileWriter"/>.
        /// </summary>
        /// <param name="videoWriter">The <see cref="IVideoFileWriter"/> to write to.</param>
        /// <param name="imageProvider">The image source.</param>
        /// <param name="frameRate">Video Frame Rate.</param>
        /// <param name="audioProvider">The audio source. null = no audio.</param>
        public Recorder(IVideoFileWriter videoWriter, IImageProvider imageProvider, int frameRate, IAudioProvider audioProvider = null)
        {
            _videoWriter = videoWriter ?? throw new ArgumentNullException(nameof(videoWriter));
            _imageProvider = imageProvider ?? throw new ArgumentNullException(nameof(imageProvider));
            _audioProvider = audioProvider;

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            if (frameRate <= 0)
                throw new ArgumentException("Frame Rate must be possitive", nameof(frameRate));

            _frameRate = frameRate;

            _continueCapturing = new ManualResetEvent(false);

            if (videoWriter.SupportsAudio && audioProvider != null)
                audioProvider.DataAvailable += AudioProvider_DataAvailable;
            else _audioProvider = null;

            _sw = new Stopwatch();

            _recordTask = Task.Factory.StartNew(async () => await DoRecord(), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Creates a new instance of <see cref="IRecorder"/> writing to <see cref="IAudioFileWriter"/>.
        /// </summary>
        /// <param name="audioWriter">The <see cref="IAudioFileWriter"/> to write to.</param>
        /// <param name="audioProvider">The audio source.</param>
        public Recorder(IAudioFileWriter audioWriter, IAudioProvider audioProvider)
        {
            _audioWriter = audioWriter ?? throw new ArgumentNullException(nameof(audioWriter));
            _audioProvider = audioProvider ?? throw new ArgumentNullException(nameof(audioProvider));

            _audioProvider.DataAvailable += AudioProvider_DataAvailable;
        }

        private Task<bool> _task;

        private async Task DoRecord()
        {
            try
            {
                var frameInterval = TimeSpan.FromSeconds(1.0 / _frameRate);
                var frameCount = 0;

                // Returns false when stopped
                bool AddFrame(IBitmapFrame frame)
                {
                    try
                    {
                        _videoWriter.WriteFrame(frame);

                        ++frameCount;

                        return true;
                    }
                    catch (InvalidOperationException)
                    {
                        return false;
                    }
                }

                bool CanContinue()
                {
                    try
                    {
                        return _continueCapturing.WaitOne();
                    }
                    catch (ObjectDisposedException)
                    {
                        return false;
                    }
                }

                while (CanContinue() && !_cancellationToken.IsCancellationRequested)
                {
                    var timestamp = DateTime.Now;

                    if (_task != null)
                    {
                        // If false, stop recording
                        if (!await _task)
                            return;

                        var requiredFrames = _sw.Elapsed.TotalSeconds * _frameRate;
                        var diff = requiredFrames - frameCount;

                        for (var i = 0; i < diff; ++i)
                        {
                            if (!AddFrame(RepeatFrame.Instance))
                                return;
                        }
                    }

                    _task = Task.Factory.StartNew(() =>
                    {
                        var editableFrame = _imageProvider.Capture();

                        if (_cancellationToken.IsCancellationRequested)
                            return false;

                        var frame = editableFrame.GenerateFrame();

                        if (_cancellationToken.IsCancellationRequested)
                            return false;

                        return AddFrame(frame);
                    });

                    var timeTillNextFrame = timestamp + frameInterval - DateTime.Now;

                    if (timeTillNextFrame > TimeSpan.Zero)
                        Thread.Sleep(timeTillNextFrame);
                }
            }
            catch (Exception e)
            {
                lock (_syncLock)
                {
                    if (!_disposed)
                    {
                        ErrorOccurred?.Invoke(e);

                        Dispose(false);
                    }
                }
            }
        }

        private void AudioProvider_DataAvailable(object sender, DataAvailableEventArgs dataAvailableEventArgs)
        {
            if (_videoWriter == null)
            {
                _audioWriter.Write(dataAvailableEventArgs.Buffer, 0, dataAvailableEventArgs.Length);
                return;
            }

            try
            {
                lock (_syncLock)
                {
                    if (_disposed)
                        return;
                }

                _videoWriter.WriteAudio(dataAvailableEventArgs.Buffer, dataAvailableEventArgs.Length);
            }
            catch (Exception e)
            {
                if (_imageProvider == null)
                {
                    lock (_syncLock)
                    {
                        if (!_disposed)
                        {
                            ErrorOccurred?.Invoke(e);

                            Dispose(true);
                        }
                    }
                }
            }
        }

        #region Dispose

        private void Dispose(bool terminateRecord)
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_audioProvider != null)
            {
                _audioProvider.DataAvailable -= AudioProvider_DataAvailable;
                _audioProvider.Stop();
                _audioProvider.Dispose();
                _audioProvider = null;
            }

            if (_videoWriter != null)
            {
                _cancellationTokenSource.Cancel();

                // Resume record loop if paused so it can exit
                _continueCapturing.Set();

                if (terminateRecord)
                    _recordTask.Wait();

                try
                {
                    if (_task != null && !_task.IsCompleted)
                    {
                        _task.GetAwaiter().GetResult();
                    }
                }
                catch
                {
                    // Ignored in dispose
                }

                _videoWriter.Dispose();
                _videoWriter = null;

                _continueCapturing.Dispose();
            }
            else
            {
                _audioWriter.Dispose();
                _audioWriter = null;
            }

            _imageProvider?.Dispose();
            _imageProvider = null;
        }

        /// <summary>
        /// Frees all resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            lock (_syncLock)
            {
                Dispose(true);
            }
        }

        private bool _disposed;

        /// <summary>
        /// Fired when an error occurs
        /// </summary>
        public event Action<Exception> ErrorOccurred;

        private void ThrowIfDisposed()
        {
            lock (_syncLock)
            {
                if (_disposed)
                    throw new ObjectDisposedException("this");
            }
        }
        #endregion

        /// <summary>
        /// Start Recording.
        /// </summary>
        public void Start()
        {
            ThrowIfDisposed();

            _sw?.Start();

            _audioProvider?.Start();
            
            _continueCapturing?.Set();
        }

        /// <summary>
        /// Stop Recording.
        /// </summary>
        public void Stop()
        {
            ThrowIfDisposed();

            _continueCapturing?.Reset();            
            _audioProvider?.Stop();

            _sw?.Stop();
        }
    }
}
