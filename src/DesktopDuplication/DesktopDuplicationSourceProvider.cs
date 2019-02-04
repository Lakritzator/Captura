using System;
using System.Linq;
using System.Text.RegularExpressions;
using Captura.Base;
using Captura.Base.Services;
using Captura.Base.Video;
using Captura.Loc;
using SharpDX.DXGI;

namespace DesktopDuplication
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DesktopDuplicationSourceProvider : NotifyPropertyChanged, IVideoSourceProvider
    {
        private readonly IVideoSourcePicker _videoSourcePicker;
        private readonly IPlatformServices _platformServices;

        public DesktopDuplicationSourceProvider(LanguageManager languageManager,
            IVideoSourcePicker videoSourcePicker,
            IIconSet icons,
            IPlatformServices platformServices)
        {
            _videoSourcePicker = videoSourcePicker;
            _platformServices = platformServices;
            Icon = icons.Game;

            languageManager.LanguageChanged += cultureInfo => RaisePropertyChanged(nameof(Name));
        }

        private bool PickScreen()
        {
            var screen = _videoSourcePicker.PickScreen();

            return screen != null && Set(screen);
        }

        private bool SelectFirst()
        {
            var output = new Factory1()
                .Adapters1
                .SelectMany(adapter1 => adapter1.Outputs
                    .Select(output1 => output1.QueryInterface<Output1>()))
                    .FirstOrDefault();

            if (output == null)
            {
                return false;
            }

            Source = new DesktopDuplicationItem(output);

            return true;
        }

        private bool Set(IScreen screen)
        {
            var outputs = new Factory1()
                            .Adapters1
                            .SelectMany(adapter1 => adapter1.Outputs
                                .Select(output => output.QueryInterface<Output1>()));

            var match = outputs.FirstOrDefault(output1 =>
            {
                var r1 = output1.Description.DesktopBounds;
                var r2 = screen.Rectangle;

                return r1.Left == r2.Left
                       && r1.Right == r2.Right
                       && r1.Top == r2.Top
                       && r1.Bottom == r2.Bottom;
            });

            if (match == null)
            {
                return false;
            }

            Source = new DesktopDuplicationItem(match);

            return true;
        }

        private IVideoItem _source;

        public IVideoItem Source
        {
            get => _source;
            private set
            {
                _source = value;
                
                OnPropertyChanged();
            }
        }

        public string Name => "Desktop Duplication";

        public string Description { get; } = @"Faster API for recording screen as well as fullscreen DirectX games.
Not all games are recordable.
Requires Windows 8 or above.
If it does not work, try running Captura on the Integrated Graphics card.";

        public string Icon { get; }

        public override string ToString() => Name;

        public bool OnSelect()
        {
            // Select first screen if there is only one
            if (_platformServices.EnumerateScreens().Count() == 1 && SelectFirst())
            {
                return true;
            }

            return PickScreen();
        }

        public void OnUnselect() { }

#pragma warning disable CS0067
        public event Action UnselectRequested;
#pragma warning restore CS0067

        public string Serialize()
        {
            return Source.ToString();
        }

        public bool Deserialize(string serialized)
        {
            var screen = _platformServices
                .EnumerateScreens()
                .FirstOrDefault(screen1 => screen1.DeviceName == serialized);

            if (screen == null)
            {
                return false;
            }

            Set(screen);

            return true;
        }

        public bool ParseCli(string arg)
        {
            if (!Regex.IsMatch(arg, @"^deskdupl:\d+$"))
            {
                return false;
            }

            var index = int.Parse(arg.Substring(9));

            var screens = _platformServices.EnumerateScreens().ToArray();

            if (index >= screens.Length)
            {
                return false;
            }

            Set(screens[index]);

            return true;
        }
    }
}