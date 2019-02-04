using System.Collections;
using System.Collections.Generic;
using Captura.Base.Services;
using Captura.Base.Video;

namespace Captura.Core.Models.Discard
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DiscardWriterProvider : IVideoWriterProvider
    {
        readonly IPreviewWindow _previewWindow;

        public DiscardWriterProvider(IPreviewWindow previewWindow)
        {
            _previewWindow = previewWindow;
        }

        public IEnumerator<IVideoWriterItem> GetEnumerator()
        {
            yield return new DiscardWriterItem(_previewWindow);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string Name { get; } = "Preview Only";

        public string Description => "For testing purposes.";

        public override string ToString() => Name;
    }
}