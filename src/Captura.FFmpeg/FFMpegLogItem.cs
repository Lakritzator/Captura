using System;
using System.Text;
using System.Windows.Input;
using Captura.Base;
using Captura.Base.Services;

namespace Captura.FFmpeg
{
    public class FFmpegLogItem : NotifyPropertyChanged
    {
        public string Name { get; }

        public FFmpegLogItem(string name, IClipboardService clipboardService)
        {
            Name = name;

            CopyToClipboardCommand = new DelegateCommand(() =>
            {
                clipboardService.SetText(_complete.ToString());
            });

            RemoveCommand = new DelegateCommand(() => RemoveRequested?.Invoke());
        }

        private string _content = "", _frame = "";

        private readonly StringBuilder _complete = new StringBuilder();

        public void Write(string text)
        {
            if (text == null)
                return;

            _complete.AppendLine(text);

            if (text.StartsWith("frame=") || text.StartsWith("size="))
            {
                Frame = text;
            }
            else Content += text + Environment.NewLine;
        }

        public string Frame
        {
            get => _frame;
            private set
            {
                _frame = value;

                OnPropertyChanged();
            }
        }

        public string Content
        {
            get => _content;
            private set
            {
                _content = value;

                OnPropertyChanged();
            }
        }

        public ICommand CopyToClipboardCommand { get; }

        public ICommand RemoveCommand { get; }

        public event Action RemoveRequested;
    }
}