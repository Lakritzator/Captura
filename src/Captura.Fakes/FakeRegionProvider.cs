using System;
using System.Drawing;
using Captura.Base.Services;
using Captura.Base.Video;
using Screna;

namespace Captura.Fakes
{
    public class FakeRegionProvider : IRegionProvider
    {
        FakeRegionProvider() { }

        public static FakeRegionProvider Instance { get; } = new FakeRegionProvider();

        public bool SelectorVisible
        {
            get => false;
            set { }
        }
        
        public Rectangle SelectedRegion { get; set; }

        public IVideoItem VideoSource => new RegionItem(this);

#pragma warning disable CS0067
        public event Action SelectorHidden;
#pragma warning restore CS0067

        public void Lock() { }

        public void Release() { }

        public IntPtr Handle => IntPtr.Zero;
    }
}
