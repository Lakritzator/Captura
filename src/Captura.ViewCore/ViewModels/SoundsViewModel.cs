using System.Collections.Generic;
using System.Linq;
using Captura.Base;
using Captura.Base.Audio;
using Captura.Base.Services;
using Captura.Core.Settings.Models;

namespace Captura.ViewCore.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SoundsViewModel : NotifyPropertyChanged
    {
        public IReadOnlyCollection<SoundsViewModelItem> Items { get; }

        public SoundsViewModel(IDialogService dialogService, SoundSettings settings)
        {
            Items = new[]
            {
                SoundKind.Start,
                SoundKind.Stop,
                SoundKind.Pause,
                SoundKind.Shot,
                SoundKind.Error,
                SoundKind.Notification
            }.Select(kind => new SoundsViewModelItem(kind, dialogService, settings)).ToList();
        }
    }
}