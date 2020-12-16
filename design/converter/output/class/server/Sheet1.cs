using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;

namespace MasterData.Table
{
    public class Sheet1
    {
        [Key]
        public string id { get; set; }
        public string name { get; set; }
        public bool? value2 { get; set; }
        [JsonConverter(typeof(JsonEnumConverter<Direction>))]
        public Direction direction { get; set; }
    }
}