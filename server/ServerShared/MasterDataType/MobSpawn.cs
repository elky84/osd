using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json.Converters;

namespace MasterData.Table
{
    public partial class MobSpawn
    {
        public string Mob { get; set; }

        public string Map { get; set; }

        public Point? Begin { get; set; }

        public Point? End { get; set; }

        public int Count { get; set; }

        public TimeSpan ZenTime { get; set; }

    }
}
