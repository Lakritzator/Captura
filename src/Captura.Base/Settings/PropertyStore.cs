using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Captura.Base.Settings
{
    public abstract class PropertyStore : NotifyPropertyChanged
    {
        readonly Dictionary<string, object> _dictionary = new Dictionary<string, object>();

        protected T Get<T>(T Default = default(T), [CallerMemberName] string propertyName = "")
        {
            lock (_dictionary)
            {
                if (_dictionary.TryGetValue(propertyName, out var obj) && obj is T val)
                {
                    return val;
                }
            }

            return Default;
        }

        protected void Set<T>(T value, [CallerMemberName] string propertyName = "")
        {
            lock (_dictionary)
            {
                if (_dictionary.ContainsKey(propertyName))
                {
                    _dictionary[propertyName] = value;
                }
                else _dictionary.Add(propertyName, value);
            }

            OnPropertyChanged(propertyName);
        }
    }
}