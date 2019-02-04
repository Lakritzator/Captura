using System;
using System.Reflection;
using Captura.Base;
using Captura.Base.Audio;
using Captura.Base.Images;
using Captura.Base.Recent;
using Captura.Base.Services;
using Captura.Base.Video;
using Captura.Bass;
using Captura.Core.Models;
using Captura.Core.Models.Discard;
using Captura.Core.Models.ImageWriterItems;
using Captura.Core.Models.Recents;
using Captura.Core.Models.UpdateCheckers;
using Captura.Core.ViewModels;
using Captura.FFmpeg;
using Captura.FFmpeg.Video;
using Captura.HotKeys;
using Captura.Imgur;
using Captura.Loc;
using Captura.MouseKeyHook;
using Captura.NAudio;
using Captura.SharpAvi;
using Captura.Windows;
using Captura.YouTube;
using DesktopDuplication;
using Screna.Frames;
using Screna.Services;
using Screna.VideoItems;
using Screna.VideoSourceProviders;

namespace Captura.Core
{
    public class CoreModule : IModule
    {
        /// <summary>
        /// Binds both as Inteface as Class
        /// </summary>
        static void BindAsInterfaceAndClass<TInterface, TClass>(IBinder binder) where  TClass : TInterface
        {
            binder.BindSingleton<TClass>();

            // ReSharper disable once ConvertClosureToMethodGroup
            binder.Bind<TInterface>(() => ServiceProvider.Get<TClass>());
        }

        public void OnLoad(IBinder binder)
        {
            BindViewModels(binder);
            BindSettings(binder);
            BindImageWriters(binder);
            BindVideoWriterProviders(binder);
            BindVideoSourceProviders(binder);
            BindAudioSource(binder);
            BindUpdateChecker(binder);

            // Recent
            binder.Bind<IRecentList, RecentListRepository>();
            binder.Bind<IRecentItemSerializer, FileRecentSerializer>();
            binder.Bind<IRecentItemSerializer, UploadRecentSerializer>();

            binder.Bind<IDialogService, DialogService>();
            binder.Bind<IClipboardService, ClipboardService>();
            binder.Bind<IImageUploader, ImgurUploader>();
            binder.Bind<IIconSet, MaterialDesignIcons>();
            binder.Bind<IImgurApiKeys, ApiKeys>();
            binder.Bind<IYouTubeApiKeys, ApiKeys>();
            binder.Bind<IPlatformServices, WindowsPlatformServices>();
            binder.Bind<IImagingSystem, DrawingImagingSystem>();

            binder.BindSingleton<FullScreenItem>();
            binder.BindSingleton<FFmpegLog>();
            binder.BindSingleton<HotKeyManager>();
            binder.Bind(() => LanguageManager.Instance);
        }

        public void Dispose() { }

        static void BindImageWriters(IBinder binder)
        {
            BindAsInterfaceAndClass<IImageWriterItem, DiskWriter>(binder);
            BindAsInterfaceAndClass<IImageWriterItem, ClipboardWriter>(binder);
            BindAsInterfaceAndClass<IImageWriterItem, ImageUploadWriter>(binder);
        }

        static void BindViewModels(IBinder binder)
        {
            binder.BindSingleton<TimerModel>();
            binder.BindSingleton<MainModel>();
            binder.BindSingleton<ScreenShotModel>();
            binder.BindSingleton<RecordingModel>();
            binder.BindSingleton<VideoSourcesViewModel>();
            binder.BindSingleton<VideoWritersViewModel>();
            binder.BindSingleton<FFmpegCodecsViewModel>();
            binder.BindSingleton<KeymapViewModel>();
        }

        static void BindUpdateChecker(IBinder binder)
        {
            var version = Assembly.GetEntryAssembly()?.GetName().Version;

            if (version?.Major == 0)
            {
                binder.Bind<IUpdateChecker, DevUpdateChecker>();
            }
            else binder.Bind<IUpdateChecker, UpdateChecker>();
        }

        static void BindAudioSource(IBinder binder)
        {
            // Check if Bass is available
            if (BassAudioSource.Available)
            {
                binder.Bind<AudioSource, BassAudioSource>();
            }
            else binder.Bind<AudioSource, NAudioSource>();
        }

        static void BindVideoSourceProviders(IBinder binder)
        {
            BindAsInterfaceAndClass<IVideoSourceProvider, NoVideoSourceProvider>(binder);
            BindAsInterfaceAndClass<IVideoSourceProvider, FullScreenSourceProvider>(binder);
            BindAsInterfaceAndClass<IVideoSourceProvider, ScreenSourceProvider>(binder);
            BindAsInterfaceAndClass<IVideoSourceProvider, WindowSourceProvider>(binder);
            BindAsInterfaceAndClass<IVideoSourceProvider, RegionSourceProvider>(binder);

            if (Windows8OrAbove)
            {
                BindAsInterfaceAndClass<IVideoSourceProvider, DesktopDuplicationSourceProvider>(binder);
            }
        }

        static void BindVideoWriterProviders(IBinder binder)
        {
            BindAsInterfaceAndClass<IVideoWriterProvider, FFmpegWriterProvider>(binder);
            // BindAsInterfaceAndClass<IVideoWriterProvider, GifWriterProvider>(Binder);
            BindAsInterfaceAndClass<IVideoWriterProvider, SharpAviWriterProvider>(binder);
            BindAsInterfaceAndClass<IVideoWriterProvider, StreamingWriterProvider>(binder);
            BindAsInterfaceAndClass<IVideoWriterProvider, DiscardWriterProvider>(binder);
        }

        static void BindSettings(IBinder binder)
        {
            binder.BindSingleton<Settings.Settings>();
            binder.Bind(() => ServiceProvider.Get<Settings.Settings>().Audio);
            binder.Bind(() => ServiceProvider.Get<Settings.Settings>().FFmpeg);
            binder.Bind(() => ServiceProvider.Get<Settings.Settings>().Gif);
            binder.Bind(() => ServiceProvider.Get<Settings.Settings>().Proxy);
            binder.Bind(() => ServiceProvider.Get<Settings.Settings>().Sounds);
            binder.Bind(() => ServiceProvider.Get<Settings.Settings>().Imgur);
        }

        static bool Windows8OrAbove
        {
            get
            {
                // All versions above Windows 8 give the same version number
                var version = new Version(6, 2, 9200, 0);

                return Environment.OSVersion.Platform == PlatformID.Win32NT &&
                       Environment.OSVersion.Version >= version;
            }
        }
    }
}