using System;
using System.Threading;
using System.Windows.Input;

namespace Captura.Base
{
    public class DelegateCommand : ICommand
    {
        readonly Action<object> _execute;
        bool _canExecute;
        readonly SynchronizationContext _syncContext = SynchronizationContext.Current;
        
        public DelegateCommand(Action<object> onExecute, bool canExecute = true)
        {
            _execute = onExecute;
            _canExecute = canExecute;
        }

        public DelegateCommand(Action onExecute, bool canExecute = true)
        {
            _execute = o => onExecute?.Invoke();
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute;

        public void Execute(object parameter) => _execute?.Invoke(parameter);

        public void RaiseCanExecuteChanged(bool canExecute)
        {
            _canExecute = canExecute;

            void Do()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            if (_syncContext != null)
            {
                _syncContext.Post(state => Do(), null);
            }
            else Do();
        }

        public event EventHandler CanExecuteChanged;
    }
}