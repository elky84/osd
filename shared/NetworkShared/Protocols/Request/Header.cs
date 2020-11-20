using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace NetworkShared.Protocols.Request
{
    public class Header
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual Id.Request Id { get; set; }

        [JsonExtensionData]
        public JObject ExtensionData { get; set; }
    }
}
