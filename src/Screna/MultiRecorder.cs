using System;
using Captura.Base;

namespace Screna
{
    public class MultiRecorder : IRecorder
    {
        private IRecorder[] _recorders;

        public MultiRecorder(params IRecorder[] recorders)
        {
            if (recorders == null || recorders.Length < 2)
            {
                throw new ArgumentException("At least two recorders required.", nameof(recorders));
            }

            _recorders = recorders;

            foreach (var recorder in recorders)
            {
                recorder.ErrorOccurred += RaiseError;
            }
        }

        private void RaiseError(Exception exception) => ErrorOccurred?.Invoke(exception);

        public void Dispose()
        {
            foreach (var recorder in _recorders)
            {
                recorder.Dispose();
                recorder.ErrorOccurred -= RaiseError;
            }

            _recorders = null;
        }

        public void Start()
        {
            foreach (var recorder in _recorders)
            {
                recorder.Start();
            }
        }

        public void Stop()
        {
            foreach (var recorder in _recorders)
            {
                recorder.Stop();
            }
        }

        public event Action<Exception> ErrorOccurred;
    }
}