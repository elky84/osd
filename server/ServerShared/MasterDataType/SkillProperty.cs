using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json.Converters;

namespace MasterData.Table
{
    public partial class SkillProperty
    {
        [Key]
        public string Id { get; set; }

        public int Level { get; set; }

        public string LevelUpFee { get; set; }

        public int HP { get; set; }

        public int MP { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Target Target { get; set; }

        public int Bound { get; set; }

        public int? TargetCount { get; set; }

        public int? HPRecovery { get; set; }

        public int? MPRecovery { get; set; }

    }
}
