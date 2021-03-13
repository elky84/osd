// Generated code. DO NOT MODIFY DIRECTLY

using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MasterData.Common
{
    public partial class SkillProperty
    {
        [NetworkShared.Util.Table.Key]
        public string Id { get; set; }

        public int Level { get; set; }

        public string LevelUpFee { get; set; }

        public int HP { get; set; }

        public int MP { get; set; }

        public Target Target { get; set; }

        public int Bound { get; set; }

        public int? TargetCount { get; set; }

        public int? HPRecovery { get; set; }

        public int? MPRecovery { get; set; }

    }
}
