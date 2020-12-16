using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;

namespace MasterData.Table
{
    public class Mob
    {
        [Key]
        public string Id { get; set; }
        [JsonConverter(typeof(JsonEnumConverter<AttackType>))]
        public AttackType AttackType { get; set; }
        public int HP { get; set; }
        public int MP { get; set; }
        public double Speed { get; set; }
    }
}