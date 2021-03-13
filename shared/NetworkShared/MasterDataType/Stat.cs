// Generated code. DO NOT MODIFY DIRECTLY

using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MasterData.Common
{
    public partial class Stat
    {
        [NetworkShared.Util.Table.Key]
        public int Level { get; set; }

        public int Hp { get; set; }

        public int Mp { get; set; }

        public int PhysicalDamage { get; set; }

        public int MagicalDamage { get; set; }

        public int Defence { get; set; }

        public int Critical { get; set; }

        public int CriticalDamage { get; set; }

        public int AttackSpeed { get; set; }

    }
}
