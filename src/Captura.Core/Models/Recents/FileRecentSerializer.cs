using System.IO;
using Captura.Base.Recent;
using Newtonsoft.Json.Linq;

namespace Captura.Core.Models.Recents
{
    public class FileRecentSerializer : IRecentItemSerializer
    {
        public bool CanSerialize(IRecentItem item) => item is FileRecentItem;

        public bool CanDeserialize(JObject item)
        {
            return item.ContainsKey(nameof(FileRecentModel.Type))
                   && item[nameof(FileRecentModel.Type)].ToString() == FileRecentModel.IdValue;
        }

        class FileRecentModel
        {
            public const string IdValue = "File";

            public string Type => IdValue;

            public RecentFileType FileType { get; set; }

            public string FileName { get; set; }
        }

        public JObject Serialize(IRecentItem item)
        {
            // Persist only if File exists or is a link.
            if (item is FileRecentItem fileRecentItem && File.Exists(fileRecentItem.FileName))
            {
                return JObject.FromObject(new FileRecentModel
                {
                    FileName = fileRecentItem.FileName,
                    FileType = fileRecentItem.FileType
                });
            }

            return null;
        }

        public IRecentItem Deserialize(JObject item)
        {
            try
            {
                var model = item.ToObject<FileRecentModel>();

                // Restore only if file exists
                return File.Exists(model.FileName)
                    ? new FileRecentItem(model.FileName, model.FileType)
                    : null;
            }
            catch
            {
                return null;
            }
        }
    }
}