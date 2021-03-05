using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json.Converters;

namespace MasterData.Table
{
    public partial class Passive
    {
        [Key]
        public string Id { get; set; }

        public int Level { get; set; }

        public int? Interval { get; set; }

        public int? Stack { get; set; }

    }
}
