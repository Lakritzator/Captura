using System;
using Captura.Base.Notification;
using Captura.Base.Services;
using Captura.Loc;

namespace Captura.Fakes
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class FakeSystemTray : ISystemTray
    {
        readonly LanguageManager _loc;

        public FakeSystemTray(LanguageManager loc)
        {
            _loc = loc;
        }

        public void HideNotification() { }

        public void ShowScreenShotNotification(string filePath)
        {
            // ReSharper disable once LocalizableElement
            Console.WriteLine($"{_loc.ScreenShotSaved}: {filePath}");
        }

        public void ShowNotification(INotification notification) { }
    }
}
