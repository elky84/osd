using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;

namespace MasterData.Table
{
    public class Item
    {
        [Key]
        public string Id { get; set; }
        [JsonConverter(typeof(JsonEnumConverter<ItemType>))]
        public ItemType Type { get; set; }
        public string ActiveScript { get; set; }
        public string InactiveScript { get; set; }
    }
}