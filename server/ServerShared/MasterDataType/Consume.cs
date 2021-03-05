using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json.Converters;

namespace MasterData.Table
{
    public partial class Consume
    {
        [Key]
        public string Group { get; set; }

        public string Item { get; set; }

        public int Count { get; set; }

    }
}
