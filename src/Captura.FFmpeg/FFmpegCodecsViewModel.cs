using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Captura.Base;
using Captura.FFmpeg.Audio;
using Captura.FFmpeg.Settings;

namespace Captura.FFmpeg
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FFmpegCodecsViewModel : NotifyPropertyChanged
    {
        public FFmpegSettings Settings { get; }

        public FFmpegCodecsViewModel(FFmpegSettings settings)
        {
            Settings = settings;

            AddCustomCodecCommand = new DelegateCommand(() => settings.CustomCodecs.Add(new CustomFFmpegCodec()));

            RemoveCustomCodecCommand = new DelegateCommand(o =>
            {
                if (o is CustomFFmpegCodec codec)
                {
                    settings.CustomCodecs.Remove(codec);
                }
            });
        }

        public ICommand AddCustomCodecCommand { get; }

        public ICommand RemoveCustomCodecCommand { get; }

        public IEnumerable<string> AudioCodecNames => FFmpegAudioItem.Items.Select(fmpegAudioItem => fmpegAudioItem.Name.Split(' ')[0]);
    }
}