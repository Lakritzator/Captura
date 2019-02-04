using System;
using Captura.Base;
using Captura.Base.Services;
using Captura.Loc;

namespace Captura.WebCam
{
    public class WebCamItem : NotifyPropertyChanged, IWebCamItem
    {
        private WebCamItem()
        {
            Name = LanguageManager.Instance.NoWebCam;

            LanguageManager.Instance.LanguageChanged += cultureInfo =>
            {
                Name = LanguageManager.Instance.NoWebCam;

                RaisePropertyChanged(nameof(Name));
            };
        }

        public WebCamItem(Filter cam)
        {
            Cam = cam ?? throw new ArgumentNullException(nameof(cam));
            Name = cam.Name;
        }

        public static WebCamItem NoWebCam { get; } = new WebCamItem();

        public Filter Cam { get; }

        public string Name { get; private set; }

        public override string ToString() => Name;
    }
}