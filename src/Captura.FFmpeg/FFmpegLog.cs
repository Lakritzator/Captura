using System.Collections.ObjectModel;
using System.Threading;
using Captura.Base;
using Captura.Base.Services;

namespace Captura.FFmpeg
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FFmpegLog : NotifyPropertyChanged
    {
        private readonly SynchronizationContext _syncContext;

        private readonly IClipboardService _clipboardService;

        public FFmpegLog(IClipboardService clipboardService)
        {
            _clipboardService = clipboardService;
            _syncContext = SynchronizationContext.Current;

            LogItems = new ReadOnlyObservableCollection<FFmpegLogItem>(_logItems);
        }

        private readonly ObservableCollection<FFmpegLogItem> _logItems = new ObservableCollection<FFmpegLogItem>();

        public ReadOnlyObservableCollection<FFmpegLogItem> LogItems { get; }

        public FFmpegLogItem CreateNew(string name)
        {
            var item = new FFmpegLogItem(name, _clipboardService);

            item.RemoveRequested += () => _logItems.Remove(item);

            void Insert()
            {
                _logItems.Insert(0, item);
            }

            if (_syncContext != null)
            {
                _syncContext.Post(state => Insert(), null);
            }
            else Insert();

            SelectedLogItem = item;

            return item;
        }

        private FFmpegLogItem _selectedLogItem;

        public FFmpegLogItem SelectedLogItem
        {
            get => _selectedLogItem;
            set
            {
                _selectedLogItem = value; 
                
                OnPropertyChanged();
            }
        }
    }
}
