using Captura.Base.Recent;
using Newtonsoft.Json.Linq;

namespace Captura.Core.Models.Recents
{
    public interface IRecentItemSerializer
    {
        bool CanSerialize(IRecentItem item);

        bool CanDeserialize(JObject item);

        JObject Serialize(IRecentItem item);

        IRecentItem Deserialize(JObject item);
    }
}