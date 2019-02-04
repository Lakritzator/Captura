using Captura.Base;

namespace Captura.Loc
{
    public class TextLocalizer : NotifyPropertyChanged
    {
        public TextLocalizer(string localizationKey)
        {
            LocalizationKey = localizationKey;

            LanguageManager.Instance.LanguageChanged += cultureInfo => RaisePropertyChanged(nameof(Display));
        }
        
        string _key;

        public string LocalizationKey
        {
            get => _key;
            set
            {
                _key = value;

                OnPropertyChanged();

                RaisePropertyChanged(nameof(Display));
            }
        }

        public string Display => ToString();

        public override string ToString() => LanguageManager.Instance[_key];
    }
}
