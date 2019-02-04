using Captura.Base;
using Captura.Loc;

namespace Captura.Core.ViewModels
{
    public abstract class ViewModelBase : NotifyPropertyChanged
    {
        protected ViewModelBase(Settings.Settings settings, LanguageManager languageManager)
        {
            Settings = settings;
            Loc = languageManager;
        }

        public Settings.Settings Settings { get; }

        public LanguageManager Loc { get; }
    }
}