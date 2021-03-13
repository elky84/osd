// Generated code. DO NOT MODIFY DIRECTLY

using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MasterData.Common
{
    public partial class Buff
    {
        [NetworkShared.Util.Table.Key]
        public string Id { get; set; }

        public int Level { get; set; }

        public int Duration { get; set; }

        public int? Interval { get; set; }

        public int? Stack { get; set; }

    }
}
