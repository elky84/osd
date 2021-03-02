using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json.Converters;

namespace MasterData.Table
{
    public partial class Stat
    {
        [Key]
        public int Level { get; set; }

        public int Hp { get; set; }

        public int Mp { get; set; }

        public int Attack { get; set; }

        public int Defence { get; set; }

        public int Critical { get; set; }

        public int CriticalDamage { get; set; }

        public int AttackSpeed { get; set; }

    }
}
