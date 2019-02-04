using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Captura.Base;
using Screna;

namespace Captura.ViewModels
{
    public class ExceptionViewModel : NotifyPropertyChanged
    {
        public ExceptionViewModel()
        {
            CopyToClipboardCommand = new DelegateCommand(() =>
            {
                if (Exceptions.Count > 0)
                {
                    Exceptions[0].ToString().WriteToClipboard();
                }
            });
        }

        private string _message = "An unhandled exception occurred. Here are the details.";

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                
                OnPropertyChanged();
            }
        }

        public void Init(Exception exception, string msg)
        {
            if (msg != null)
                Message = msg;

            while (exception != null)
            {
                Exceptions.Add(exception);

                exception = exception.InnerException;
            }

            SelectedException = Exceptions[0];
        }

        public ObservableCollection<Exception> Exceptions { get; } = new ObservableCollection<Exception>();

        private Exception _selectedException;

        public Exception SelectedException
        {
            get => _selectedException;
            set
            {
                _selectedException = value;
                
                OnPropertyChanged();
            }
        }

        public ICommand CopyToClipboardCommand { get; }
    }
}