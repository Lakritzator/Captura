using System.Collections.ObjectModel;
using System.Windows.Input;
using Captura.Base;
using Captura.Base.Recent;
using Captura.Core.Settings;
using Captura.Core.ViewModels;
using Captura.Loc;

namespace Captura.ViewCore.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RecentViewModel : ViewModelBase
    {
        public ReadOnlyObservableCollection<IRecentItem> Items { get; }

        public ICommand ClearCommand { get; }

        public RecentViewModel(Settings settings,
            LanguageManager languageManager,
            IRecentList recent)
            : base(settings, languageManager)
        {
            Items = recent.Items;

            ClearCommand = new DelegateCommand(recent.Clear);
        }
    }
}