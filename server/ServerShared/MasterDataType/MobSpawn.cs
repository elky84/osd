// Generated code. DO NOT MODIFY DIRECTLY

using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MasterData.Server
{
    public partial class MobSpawn : MasterData.Common.MobSpawn
    {
        public string Mob { get; set; }

        public string Map { get; set; }

        public Point? Begin { get; set; }

        public Point? End { get; set; }

        public int Count { get; set; }

        public TimeSpan ZenTime { get; set; }

    }
}
