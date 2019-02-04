using System.Linq;
using System.Text.RegularExpressions;
using Captura.Base;
using Captura.Base.Services;
using Captura.Base.Video;
using Captura.Loc;
using Screna.VideoItems;

namespace Screna.VideoSourceProviders
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ScreenSourceProvider : VideoSourceProviderBase
    {
        private readonly IVideoSourcePicker _videoSourcePicker;
        private readonly IPlatformServices _platformServices;
        
        public ScreenSourceProvider(LanguageManager loc,
            IVideoSourcePicker videoSourcePicker,
            IIconSet icons,
            IPlatformServices platformServices) : base(loc)
        {
            _videoSourcePicker = videoSourcePicker;
            _platformServices = platformServices;

            Icon = icons.Screen;
        }

        private bool PickScreen()
        {
            var screen = _videoSourcePicker.PickScreen();

            if (screen == null)
                return false;

            _source = new ScreenItem(screen);
            RaisePropertyChanged(nameof(Source));
            return true;
        }

        private void Set(IScreen screen)
        {
            _source = new ScreenItem(screen);
            RaisePropertyChanged(nameof(Source));
        }

        private IVideoItem _source;

        public override IVideoItem Source => _source;

        public override string Name => Loc.Screen;

        public override string Description { get; } = "Record a specific screen.";

        public override string Icon { get; }

        public override bool OnSelect()
        {
            var screens = _platformServices.EnumerateScreens().ToArray();

            // Select first screen if there is only one
            if (screens.Length == 1)
            {
                Set(screens[0]);
                return true;
            }

            return PickScreen();
        }

        public override bool Deserialize(string serialized)
        {
            var screen = _platformServices.EnumerateScreens()
                .FirstOrDefault(screen1 => screen1.DeviceName == serialized);

            if (screen == null)
                return false;

            Set(screen);

            return true;
        }

        public override bool ParseCli(string arg)
        {
            if (!Regex.IsMatch(arg, @"^screen:\d+$"))
                return false;

            var index = int.Parse(arg.Substring(7));

            var screens = _platformServices.EnumerateScreens().ToArray();

            if (index >= screens.Length)
                return false;

            Set(screens[index]);

            return true;
        }
    }
}