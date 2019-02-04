using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Captura.Base;
using Captura.MouseKeyHook.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Captura.MouseKeyHook
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class KeymapViewModel : NotifyPropertyChanged
    {
        Keymap _keymap;

        public const string DefaultKeymapFileName = "en";

        public class KeymapItem
        {
            public KeymapItem(string fileName, string name)
            {
                FileName = fileName;
                Name = name;
            }

            public string FileName { get; }

            public string Name { get; }
        }

        public KeymapViewModel()
        {
            var location = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var keymapDir = string.IsNullOrEmpty(location) ? null : Path.Combine(location, "keymaps");

            if (Directory.Exists(keymapDir))
            {
                var files = Directory.EnumerateFiles(keymapDir, "*.json");

                _keymaps.AddRange(files
                    .Where(s => !s.Contains("schema"))
                    .Select(fileName =>
                    {
                        var content = File.ReadAllText(fileName);

                        var name = JObject.Parse(content)["Name"];

                        return new KeymapItem(fileName, name.ToString());
                    }));
            }

            if (AvailableKeymaps.Count == 0)
            {
                var empty = new KeymapItem("", "Empty");

                _keymap = new Keymap();

                _keymaps.Add(empty);

                _selectedKeymap = empty;
            }
            else SelectedKeymap = AvailableKeymaps[0];
        }

        readonly List<KeymapItem> _keymaps = new List<KeymapItem>();

        public IReadOnlyList<KeymapItem> AvailableKeymaps => _keymaps;

        KeymapItem _selectedKeymap;

        public KeymapItem SelectedKeymap
        {
            get => _selectedKeymap;
            set
            {
                if (!File.Exists(value.FileName))
                    return;

                try
                {
                    Parse(File.ReadAllText(value.FileName));

                    _selectedKeymap = value;

                    OnPropertyChanged();
                }
                catch
                {
                    // Ignore errors
                }
            }
        }

        void Init(Keymap keymap)
        {
            _keymap = keymap;

            Control = Find(Keys.Control, ModifierStates.Empty) ?? nameof(Control);
            Shift = Find(Keys.Shift, ModifierStates.Empty) ?? nameof(Shift);
            Alt = Find(Keys.Alt, ModifierStates.Empty) ?? nameof(Alt);
        }

        void Parse(string content)
        {
            var keymap = new Keymap();

            JsonConvert.PopulateObject(content, keymap);
            
            Init(keymap);
        }

        public string Find(Keys key, ModifierStates modifiers)
        {
            return _keymap.Mappings
                .Where(mappingGroup => mappingGroup.On.Any(modifierStates => modifierStates.Control == modifiers.Control
                              && modifierStates.Alt == modifiers.Alt
                              && modifierStates.Shift == modifiers.Shift
                              && modifierStates.CapsLock == modifiers.CapsLock))
                .SelectMany(mappingGroup => mappingGroup.Keys)
                .FirstOrDefault(keyValuePair => keyValuePair.Key == key).Value;
        }

        public string Control { get; private set; } = nameof(Control);

        public string Shift { get; private set; } = nameof(Shift);

        public string Alt { get; private set; } = nameof(Alt);
    }
}