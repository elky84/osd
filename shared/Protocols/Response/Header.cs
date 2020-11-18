using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Protocols.Code;

namespace Protocols.Response
{
    public class Header
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual Id.Response Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Result Result { get; set; }

        [JsonExtensionData]
        public JObject ExtensionData { get; set; }
    }
}
