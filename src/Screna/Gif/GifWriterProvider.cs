using System.Collections;
using System.Collections.Generic;
using Captura.Base;
using Captura.Base.Video;
using Captura.Loc;

namespace Screna.Gif
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class GifWriterProvider : NotifyPropertyChanged, IVideoWriterProvider
    {
        private readonly GifItem _gifItem;

        public GifWriterProvider(LanguageManager loc, GifItem gifItem)
        {
            _gifItem = gifItem;

            loc.LanguageChanged += cultureInfo => RaisePropertyChanged(nameof(Name));
        }
        
        public string Name => "Gif";

        public IEnumerator<IVideoWriterItem> GetEnumerator()
        {
            yield return _gifItem;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => Name;

        public string Description => @"Use internal Gif encoder.
Variable Frame Rate mode is recommended.";
    }
}
