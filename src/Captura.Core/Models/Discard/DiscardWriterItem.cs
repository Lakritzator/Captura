using Captura.Base.Services;
using Captura.Base.Video;

namespace Captura.Core.Models.Discard
{
    public class DiscardWriterItem : IVideoWriterItem
    {
        readonly IPreviewWindow _previewWindow;

        public DiscardWriterItem(IPreviewWindow previewWindow)
        {
            _previewWindow = previewWindow;
        }

        public string Extension { get; } = "";

        public IVideoFileWriter GetVideoFileWriter(VideoWriterArgs args)
        {
            _previewWindow.Show();

            return new DiscardWriter();
        }

        public override string ToString() => "Preview";

        public string Description => "For testing purposes.";
    }
}