using System;

namespace Captura.Base.Video
{
    public interface IVideoSourceProvider
    {
        string Name { get; }

        string Description { get; }

        string Icon { get; }

        IVideoItem Source { get; }

        bool OnSelect();

        void OnUnselect();

        event Action UnselectRequested;

        string Serialize();

        bool Deserialize(string serialized);

        bool ParseCli(string arg);
    }
}