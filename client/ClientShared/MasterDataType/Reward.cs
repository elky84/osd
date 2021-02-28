using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json.Converters;

namespace MasterData.Table
{
    public partial class Reward
    {
        [Key]
        public string Id { get; set; }

        public string Group { get; set; }

        public string Item { get; set; }

        public int Weight { get; set; }

    }
}
