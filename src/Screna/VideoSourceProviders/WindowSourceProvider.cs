using System;
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
    public class WindowSourceProvider : VideoSourceProviderBase
    {
        private readonly IVideoSourcePicker _videoSourcePicker;
        private readonly IRegionProvider _regionProvider;
        private readonly IPlatformServices _platformServices;

        public WindowSourceProvider(LanguageManager loc,
            IVideoSourcePicker videoSourcePicker,
            IRegionProvider regionProvider,
            IIconSet icons,
            IPlatformServices platformServices) : base(loc)
        {
            _videoSourcePicker = videoSourcePicker;
            _regionProvider = regionProvider;
            _platformServices = platformServices;

            Icon = icons.Window;
        }

        private bool PickWindow()
        {
            var window = _videoSourcePicker.PickWindow(window1 => window1.Handle != _regionProvider.Handle);

            if (window == null)
                return false;

            _source = new WindowItem(window);

            RaisePropertyChanged(nameof(Source));
            return true;
        }

        private void Set(IWindow window)
        {
            _source = new WindowItem(window);
            RaisePropertyChanged(nameof(Source));
        }

        private IVideoItem _source;

        public override IVideoItem Source => _source;

        public override string Name => Loc.Window;

        public override string Description { get; } = @"Record a specific window.
The video is of the initial size of the window.";

        public override string Icon { get; }

        public override bool OnSelect()
        {
            return PickWindow();
        }

        public override bool Deserialize(string serialized)
        {
            var window = _platformServices.EnumerateWindows()
                .FirstOrDefault(window1 => window1.Title == serialized);

            if (window == null)
                return false;

            Set(window);

            return true;
        }

        public override bool ParseCli(string arg)
        {
            if (!Regex.IsMatch(arg, @"^win:\d+$"))
                return false;

            var handle = new IntPtr(int.Parse(arg.Substring(4)));

            Set(_platformServices.GetWindow(handle));

            return true;
        }
    }
}