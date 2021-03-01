using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json.Converters;

namespace MasterData.Table
{
    public partial class Mob
    {
        [Key]
        public string Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AttackType AttackType { get; set; }

        public int HP { get; set; }

        public int MP { get; set; }

        public double Speed { get; set; }

        public int Damage { get; set; }

        public List<string> Rewards { get; set; }

    }
}
