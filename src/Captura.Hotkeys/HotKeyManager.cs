using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Captura.Base;
using Captura.Base.Services;
using Newtonsoft.Json;

namespace Captura.HotKeys
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class HotKeyManager : IDisposable
    {
        private readonly ObservableCollection<HotKey> _hotKeys = new ObservableCollection<HotKey>();

        public ReadOnlyObservableCollection<HotKey> HotKeys { get; }

        private static string GetFilePath() => Path.Combine(ServiceProvider.SettingsDir, "HotKeys.json");

        public ICommand ResetCommand { get; }

        public ICommand AddCommand { get; }

        public ICommand RemoveCommand { get; }

        public HotKeyManager()
        {
            HotKeys = new ReadOnlyObservableCollection<HotKey>(_hotKeys);

            ResetCommand = new DelegateCommand(Reset);

            AddCommand = new DelegateCommand(Add);

            RemoveCommand = new DelegateCommand(Remove);
        }

        private void Remove(object o)
        {
            if (!(o is HotKey hotKey))
            {
                return;
            }

            hotKey.UnRegister();

            _hotKeys.Remove(hotKey);
        }

        private void Add()
        {
            var hotKey = new HotKey(new HotKeyModel(ServiceName.None, Keys.None, Modifiers.None, false));

            _hotKeys.Add(hotKey);
        }

        public static IEnumerable<Service> AllServices { get; } = Enum
            .GetValues(typeof(ServiceName))
            .Cast<ServiceName>()
            .Select(serviceName => new Service(serviceName))
            .ToArray(); // Prevent multiple enumerations

        public void RegisterAll()
        {
            IEnumerable<HotKeyModel> models;

            try
            {
                var json = File.ReadAllText(GetFilePath());

                models = JsonConvert.DeserializeObject<IEnumerable<HotKeyModel>>(json);
            }
            catch
            {
                models = Defaults();
            }

            Populate(models);
        }

        public void Reset()
        {
            Dispose();

            _hotKeys.Clear();

            Populate(Defaults());
        }

        private void Populate(IEnumerable<HotKeyModel> models)
        {
            foreach (var model in models)
            {
                var hotKey = new HotKey(model);

                if (hotKey.IsActive && !hotKey.IsRegistered)
                    _notRegisteredOnStartup.Add(hotKey);

                _hotKeys.Add(hotKey);
            }
        }

        private readonly List<HotKey> _notRegisteredOnStartup = new List<HotKey>();

        public void ShowNotRegisteredOnStartup()
        {
            if (_notRegisteredOnStartup.Count > 0)
            {
                var message = "The following HotKeys could not be registered:\nOther programs might be using them.\nTry changing them.\n\n";

                foreach (var hotKey in _notRegisteredOnStartup)
                {
                    message += $"{hotKey.Service.Description} - {hotKey}\n\n";
                }

                ServiceProvider.MessageProvider.ShowError(message, "Failed to Register HotKeys");

                _notRegisteredOnStartup.Clear();
            }
        }

        private static IEnumerable<HotKeyModel> Defaults()
        {
            return new[]
            {
                new HotKeyModel(ServiceName.Recording, Keys.F9, Modifiers.Alt, true),
                new HotKeyModel(ServiceName.Pause, Keys.F9, Modifiers.Shift, true),
                new HotKeyModel(ServiceName.ScreenShot, Keys.PrintScreen, 0, true),
                new HotKeyModel(ServiceName.ActiveScreenShot, Keys.PrintScreen, Modifiers.Alt, true),
                new HotKeyModel(ServiceName.DesktopScreenShot, Keys.PrintScreen, Modifiers.Shift, true)
            };
        }
        
        public void ProcessHotKey(int id)
        {
            var hotKey = HotKeys.SingleOrDefault(key => key.Id == id);

            if (hotKey != null)
                HotKeyPressed?.Invoke(hotKey.Service.ServiceName);
        }

        public void FakeHotKey(ServiceName service)
        {
            HotKeyPressed?.Invoke(service);
        }

        public event Action<ServiceName> HotKeyPressed;
        
        public void Dispose()
        {
            var models = HotKeys.Select(hotKey =>
            {
                hotKey.UnRegister();

                return new HotKeyModel(hotKey.Service.ServiceName, hotKey.Key, hotKey.Modifiers, hotKey.IsActive);
            });

            try
            {
                var json = JsonConvert.SerializeObject(models);

                File.WriteAllText(GetFilePath(), json);
            }
            catch
            {
                // Ignore Errors
            }
        }
    }
}