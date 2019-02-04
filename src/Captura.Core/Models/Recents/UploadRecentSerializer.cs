using System.Collections.Generic;
using System.Linq;
using Captura.Base;
using Captura.Base.Recent;
using Newtonsoft.Json.Linq;

namespace Captura.Core.Models.Recents
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UploadRecentSerializer : IRecentItemSerializer
    {
        readonly IEnumerable<IImageUploader> _imgUploaders;

        public UploadRecentSerializer(IEnumerable<IImageUploader> imgUploaders)
        {
            _imgUploaders = imgUploaders;
        }

        public bool CanSerialize(IRecentItem item) => item is UploadRecentItem;

        public bool CanDeserialize(JObject item)
        {
            return item.ContainsKey(nameof(UploadRecentModel.Type))
                   && item[nameof(UploadRecentModel.Type)].ToString() == UploadRecentModel.IdValue;
        }

        class UploadRecentModel
        {
            public const string IdValue = "Upload";

            public string Type => IdValue;

            public string Link { get; set; }

            public string DeleteHash { get; set; }

            public string UploaderService { get; set; }
        }

        public JObject Serialize(IRecentItem item)
        {
            if (item is UploadRecentItem uploadRecentItem)
            {
                return JObject.FromObject(new UploadRecentModel
                {
                    Link = uploadRecentItem.Link,
                    DeleteHash = uploadRecentItem.DeleteHash,
                    UploaderService = uploadRecentItem.UploaderService.UploadServiceName
                });
            }

            return null;
        }

        public IRecentItem Deserialize(JObject item)
        {
            try
            {
                var model = item.ToObject<UploadRecentModel>();

                var uploader = _imgUploaders.FirstOrDefault(imageUploader => imageUploader.UploadServiceName == model.UploaderService);

                return uploader != null
                    ? new UploadRecentItem(model.Link, model.DeleteHash, uploader)
                    : null;
            }
            catch
            {
                return null;
            }
        }
    }
}