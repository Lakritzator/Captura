using System;
using System.Drawing;
using Captura.Base.Video;

namespace Captura.Base.Services
{
    public interface IRegionProvider
    {
        bool SelectorVisible { get; set; }

        Rectangle SelectedRegion { get; set; }

        IVideoItem VideoSource { get; }
        
        void Lock();

        void Release();

        event Action SelectorHidden;

        IntPtr Handle { get; }
    }
}
