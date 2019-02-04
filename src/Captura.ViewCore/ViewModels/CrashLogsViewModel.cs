using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Captura.Base;
using Captura.Base.Services;
using Screna;

namespace Captura.ViewCore.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CrashLogsViewModel : NotifyPropertyChanged
    {
        public CrashLogsViewModel()
        {
            var folder = Path.Combine(ServiceProvider.SettingsDir, "Crashes");

            if (Directory.Exists(folder))
            {
                CrashLogs = new ObservableCollection<FileContentItem>(Directory
                    .EnumerateFiles(folder)
                    .Select(fileName => new FileContentItem(fileName))
                    .Reverse());

                if (CrashLogs.Count > 0)
                {
                    SelectedCrashLog = CrashLogs[0];
                }
            }

            CopyToClipboardCommand = new DelegateCommand(() => SelectedCrashLog?.Content.WriteToClipboard());

            RemoveCommand = new DelegateCommand(OnRemoveExecute);
        }

        private void OnRemoveExecute()
        {
            if (SelectedCrashLog != null)
            {
                if (File.Exists(SelectedCrashLog.FileName))
                {
                    File.Delete(SelectedCrashLog.FileName);
                }

                CrashLogs.Remove(SelectedCrashLog);

                SelectedCrashLog = CrashLogs.Count > 0 ? CrashLogs[0] : null;
            }
        }

        public ObservableCollection<FileContentItem> CrashLogs { get; }

        private FileContentItem _selectedCrashLog;

        public FileContentItem SelectedCrashLog
        {
            get => _selectedCrashLog;
            set
            {
                _selectedCrashLog = value;
                
                OnPropertyChanged();
            }
        }

        public ICommand CopyToClipboardCommand { get; }

        public ICommand RemoveCommand { get; }
    }
}