using System.IO;
using System.Windows.Input;
using Captura.Base;
using Captura.Base.Audio;
using Captura.Base.Services;
using Captura.Core.Settings.Models;

namespace Captura.ViewCore
{
    public class SoundsViewModelItem : NotifyPropertyChanged
    {
        private readonly SoundSettings _settings;

        public SoundsViewModelItem(SoundKind soundKind, IDialogService dialogService, SoundSettings settings)
        {
            SoundKind = soundKind;
            _settings = settings;

            ResetCommand = new DelegateCommand(() => FileName = null);

            SetCommand = new DelegateCommand(() =>
            {
                var folder = dialogService.PickFile(Path.GetDirectoryName(FileName), "");

                if (folder != null)
                    FileName = folder;
            });
        }

        public string FileName
        {
            get => _settings.Items.TryGetValue(SoundKind, out var value) ? value : null;
            set
            {
                if (_settings.Items.ContainsKey(SoundKind))
                {
                    _settings.Items[SoundKind] = value;
                }
                else _settings.Items.Add(SoundKind, value);

                OnPropertyChanged();
            }
        }

        public SoundKind SoundKind { get; }

        public ICommand ResetCommand { get; }

        public ICommand SetCommand { get; }
    }
}