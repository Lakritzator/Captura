using System.IO;
using System.Linq;
using System.Reflection;
using Captura.Base;

namespace Captura.ViewCore.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LicensesViewModel : NotifyPropertyChanged
    {
        public LicensesViewModel()
        {
            var selfPath = Assembly.GetEntryAssembly().Location;

            string directoryName = Path.GetDirectoryName(selfPath);
            if (string.IsNullOrEmpty(directoryName))
            {
                return;
            }
            var folder = Path.Combine(directoryName, "licenses");

            if (!Directory.Exists(folder))
            {
                return;
            }

            Licenses = Directory.EnumerateFiles(folder).Select(fileName => new FileContentItem(fileName)).ToArray();

            if (Licenses.Length > 0)
            {
                SelectedLicense = Licenses[0];
            }
        }

        public FileContentItem[] Licenses { get; }

        private FileContentItem _selectedLicense;

        public FileContentItem SelectedLicense
        {
            get => _selectedLicense;
            set
            {
                _selectedLicense = value;
                
                OnPropertyChanged();
            }
        }
    }
}