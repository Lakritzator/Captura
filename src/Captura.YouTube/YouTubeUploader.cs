using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Captura.Base.Settings;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace Captura.YouTube
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class YouTubeUploader
    {
        readonly IYouTubeApiKeys _apiKeys;
        readonly ProxySettings _proxySettings;

        YouTubeService _youtubeService;

        public YouTubeUploader(IYouTubeApiKeys apiKeys,
            ProxySettings proxySettings)
        {
            _apiKeys = apiKeys;
            _proxySettings = proxySettings;
        }

        static string GetPrivacyStatus(YouTubePrivacyStatus privacyStatus)
        {
            return privacyStatus.ToString().ToLower();
        }

        public async Task<YouTubeUploadRequest> CreateUploadRequest(string fileName,
            string title,
            string description,
            string[] tags = null,
            YouTubePrivacyStatus privacyStatus = YouTubePrivacyStatus.Unlisted)
        {
            if (_youtubeService == null)
                await Init();

            var video = new Video
            {
                Snippet = new VideoSnippet
                {
                    Title = title,
                    Description = description,
                    Tags = tags ?? new string[0],
                    CategoryId = "22"
                },
                Status = new VideoStatus { PrivacyStatus = GetPrivacyStatus(privacyStatus) }
            };

            return new YouTubeUploadRequest(fileName, _youtubeService, video);
        }

        async Task Init()
        {
            WebRequest.DefaultWebProxy = _proxySettings.GetWebProxy();

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync
            (
                new ClientSecrets
                {
                    ClientId = _apiKeys.YouTubeClientId,
                    ClientSecret = _apiKeys.YouTubeClientSecret
                },
                // This OAuth 2.0 access scope allows an application to upload files to the
                // authenticated user's YouTube channel, but doesn't allow other types of access.
                new[] { YouTubeService.Scope.YoutubeUpload },
                "user",
                CancellationToken.None
            );

            _youtubeService = new YouTubeService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = nameof(Captura)
            });
        }
    }
}