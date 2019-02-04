using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace Captura.YouTube
{
    public class YouTubeUploadRequest : IDisposable
    {
        readonly VideosResource.InsertMediaUpload _videoInsertRequest;
        readonly Stream _dataStream;

        internal YouTubeUploadRequest(string fileName,
            YouTubeService youTubeService,
            Video video)
        {
            _dataStream = new FileStream(fileName, FileMode.Open);
            _videoInsertRequest = youTubeService.Videos.Insert(video, "snippet,status", _dataStream, "video/*");

            _videoInsertRequest.ProgressChanged += VideosInsertRequest_ProgressChanged;
            _videoInsertRequest.ResponseReceived += VideosInsertRequest_ResponseReceived;
        }

        void VideosInsertRequest_ProgressChanged(IUploadProgress progress)
        {
            BytesSent?.Invoke(progress.BytesSent);
        }

        void VideosInsertRequest_ResponseReceived(Video video)
        {
            Uploaded?.Invoke($"https://youtube.com/watch?v={video.Id}");
        }

        public async Task<IUploadProgress> Upload(CancellationToken cancellationToken)
        {
            return await _videoInsertRequest.UploadAsync(cancellationToken);
        }

        public async Task<IUploadProgress> Resume(CancellationToken cancellationToken)
        {
            return await _videoInsertRequest.ResumeAsync(cancellationToken);
        }

        public event Action<long> BytesSent;

        public event Action<string> Uploaded;

        public void Dispose()
        {
            _dataStream.Dispose();
        }
    }
}