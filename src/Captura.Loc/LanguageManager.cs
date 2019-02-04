using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Captura.Loc
{
    public class LanguageManager : LanguageFields
    {
        readonly JObject _defaultLanguage;
        JObject _currentLanguage;
        readonly string _langDir;
        
        public static LanguageManager Instance { get; } = new LanguageManager();

        LanguageManager()
        {
            var entryLocation = Assembly.GetEntryAssembly()?.Location;

            var cultures = new List<CultureInfo>();

            if (entryLocation != null)
            {
                var location = Path.GetDirectoryName(entryLocation);
                _langDir = string.IsNullOrEmpty(location) ? null : Path.Combine(location, "Languages");

                if (Directory.Exists(_langDir))
                {
                    foreach (var file in Directory.EnumerateFiles(_langDir, "*.json"))
                    {
                        var cultureName = Path.GetFileNameWithoutExtension(file);

                        try
                        {
                            if (cultureName == null)
                                continue;

                            var culture = CultureInfo.GetCultureInfo(cultureName);

                            cultures.Add(culture);
                        }
                        catch
                        {
                            // Ignore
                        }
                    }
                }
            }

            _defaultLanguage = LoadLang("en");

            if (_currentCulture == null)
                CurrentCulture = new CultureInfo("en");

            if (cultures.Count == 0)
                cultures.Add(CurrentCulture);

            cultures.Sort((cultureInfo, y) => string.Compare(cultureInfo.DisplayName, y.DisplayName, StringComparison.Ordinal));

            AvailableCultures = cultures;
        }

        public IReadOnlyList<CultureInfo> AvailableCultures { get; }

        CultureInfo _currentCulture;

        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                _currentCulture = value;

                Thread.CurrentThread.CurrentUICulture = value;

                _currentLanguage = LoadLang(value.Name);

                LanguageChanged?.Invoke(value);

                RaiseAllChanged();
            }
        }

        public event Action<CultureInfo> LanguageChanged;

        JObject LoadLang(string languageId)
        {
            try
            {
                var filePath = Path.Combine(_langDir, $"{languageId}.json");

                return JObject.Parse(File.ReadAllText(filePath));
            }
            catch
            {
                return new JObject();
            }
        }

        public string this[string key]
        {
            get
            {
                if (key == null)
                    return "";

                if (_currentLanguage != null
                    && _currentLanguage.TryGetValue(key, out var value)
                    && value.ToString() is string s
                    && !string.IsNullOrWhiteSpace(s))
                    return s;

                if (_defaultLanguage != null
                    && _defaultLanguage.TryGetValue(key, out value)
                    && value.ToString() is string t
                    && !string.IsNullOrWhiteSpace(t))
                    return t;

                return key;
            }
        }

        protected override string GetValue(string propertyName) => this[propertyName];
    }
}
