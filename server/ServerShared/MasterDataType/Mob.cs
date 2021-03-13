// Generated code. DO NOT MODIFY DIRECTLY

using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MasterData.Server
{
    public partial class Mob : MasterData.Common.Mob
    {
        public AttackType AttackType { get; set; }

        public int HP { get; set; }

        public int Defence { get; set; }

        public double Speed { get; set; }

        public int Damage { get; set; }

        public List<string> Rewards { get; set; }

    }
}
