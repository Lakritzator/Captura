using Newtonsoft.Json;

namespace Captura.Imgur
{
    internal class ImgurUploadResponse : ImgurResponse
    {
        [JsonProperty("data")]
        public ImgurData Data { get; set; }
    }
}