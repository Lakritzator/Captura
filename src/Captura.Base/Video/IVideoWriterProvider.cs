using System.Collections.Generic;

namespace Captura.Base.Video
{
    public interface IVideoWriterProvider : IEnumerable<IVideoWriterItem>
    {
        string Name { get; }

        string Description { get; }
    }
}