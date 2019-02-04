using System.Linq;
using Captura.Base.Services;
using Captura.Base.Video;
using Captura.FFmpeg.Audio;
using Captura.Loc;
using Screna.VideoItems;
using Screna.VideoSourceProviders;

namespace Captura.Core.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class NoVideoSourceProvider : VideoSourceProviderBase
    {
        public NoVideoSourceProvider(LanguageManager loc,
            IIconSet icons) : base(loc)
        {
            Sources = new IVideoItem[] {WaveItem.Instance}
                .Concat(FFmpegAudioItem.Items)
                .ToArray();

            Icon = icons.NoVideo;
        }

        public IVideoItem[] Sources { get; }

        IVideoItem _selectedSource = WaveItem.Instance;

        public IVideoItem SelectedSource
        {
            get => _selectedSource;
            set
            {
                _selectedSource = value;
                
                OnPropertyChanged();

                RaisePropertyChanged(nameof(Source));
            }
        }

        public override IVideoItem Source => _selectedSource;

        public override string Name => Loc.OnlyAudio;

        public override string Description { get; } = @"No Video recorded.
Can be used for audio-only recording.
Make sure Audio sources are enabled.";

        public override string Icon { get; }

        public override bool Deserialize(string serialized)
        {
            var source = Sources.FirstOrDefault(videoItem => videoItem.Name == serialized);

            if (source == null)
                return false;

            SelectedSource = source;

            return true;
        }

        public override bool ParseCli(string arg)
        {
            return arg == "none";
        }
    }
}