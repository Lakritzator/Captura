using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Services;
using Captura.Core.Models.ImageWriterItems;
using Captura.Loc;
using Screna;

namespace Captura.Core
{
    public static class Extensions
    {
        public static void ExecuteIfCan(this ICommand command)
        {
            if (command.CanExecute(null))
                command.Execute(null);
        }

        public static async Task UploadImage(this IBitmapImage bitmap)
        {
            var uploadWriter = ServiceProvider.Get<ImageUploadWriter>();

            var settings = ServiceProvider.Get<Settings.Settings>();

            var response = await uploadWriter.Save(bitmap, settings.ScreenShots.ImageFormat);

            switch (response)
            {
                case Exception ex:
                    var loc = ServiceProvider.Get<LanguageManager>();
                    ServiceProvider.MessageProvider.ShowException(ex, loc.ImageUploadFailed);
                    break;

                case UploadResult uploadResult:
                    uploadResult.Url.WriteToClipboard();
                    break;
            }
        }
    }
}