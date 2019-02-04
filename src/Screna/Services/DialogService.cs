using System.Windows.Forms;
using Captura.Base.Services;
using Ookii.Dialogs;

namespace Screna.Services
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DialogService : IDialogService
    {
        public string PickFolder(string current, string description)
        {
            using (var dlg = new VistaFolderBrowserDialog
            {
                SelectedPath = current,
                UseDescriptionForTitle = true,
                Description = description
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    return dlg.SelectedPath;
            }

            return null;
        }

        public string PickFile(string initialFolder, string description)
        {
            var ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = initialFolder,
                Title = description
            };

            return ofd.ShowDialog() == DialogResult.OK ? ofd.FileName : null;
        }
    }
}