using System.Collections.Generic;
using Captura.Base.Audio;
using Captura.Base.Settings;

namespace Captura.Core.Settings.Models
{
    public class SoundSettings : PropertyStore
    {
        public Dictionary<SoundKind, string> Items { get; } = new Dictionary<SoundKind, string>();
    }
}