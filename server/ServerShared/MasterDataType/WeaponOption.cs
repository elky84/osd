using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json.Converters;

namespace MasterData.Table
{
    public partial class WeaponOption
    {
        [Key]
        public string Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WeaponType Type { get; set; }

        public int PhysicalDamage { get; set; }

        public int MagicalDamage { get; set; }

        public double CriticalDamage { get; set; }

        public double AttackSpeed { get; set; }

    }
}
