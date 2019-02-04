using System;
using System.Collections.Generic;
using Captura.Base.Services;
using Captura.Base.Video;
using Captura.FFmpeg.Settings;

namespace Captura.FFmpeg.Video
{
    public class StreamingItem : FFmpegItem
    {
        private readonly FFmpegItem _baseItem;
        private readonly Func<string> _linkFunction;

        private StreamingItem(string name, Func<string> linkFunction, FFmpegItem baseItem, string description) : base(name, () => baseItem.Extension, description)
        {
            _baseItem = baseItem;
            _linkFunction = linkFunction;
        }

        public override IVideoFileWriter GetVideoFileWriter(VideoWriterArgs args)
        {
            args.FileName = _linkFunction();

            return _baseItem.GetVideoFileWriter(args, "-g 20 -r 10 -f flv");
        }
        
        public static IEnumerable<StreamingItem> StreamingItems { get; } = new[]
        {
            new StreamingItem("Twitch", () =>
            {
                var settings = ServiceProvider.Get<FFmpegSettings>();

                return $"rtmp://live.twitch.tv/app/{settings.TwitchKey}";
            }, x264, "Stream to Twitch"),
            new StreamingItem("YouTube Live", () =>
            {
                var settings = ServiceProvider.Get<FFmpegSettings>();

                return $"rtmp://a.rtmp.youtube.com/live2/{settings.YouTubeLiveKey}";
            }, x264, "Stream to YouTube Live (Not Tested)"),
            new StreamingItem("Custom", () =>
            {
                var settings = ServiceProvider.Get<FFmpegSettings>();

                return settings.CustomStreamingUrl;
            }, x264, "Stream to custom service")
        };
    }
}