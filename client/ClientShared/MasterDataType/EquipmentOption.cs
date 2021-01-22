using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;

namespace MasterData.Table
{
    public partial class EquipmentOption
    {
        [Key]
        public string Id { get; set; }

        [JsonConverter(typeof(JsonEnumConverter<EquipmentType>))]
        public EquipmentType Type { get; set; }

        public int HP { get; set; }

        public int MP { get; set; }

        public int Defence { get; set; }

    }
}
