using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json.Converters;

namespace MasterData.Table
{
    public partial class Map
    {
        [Key]
        public string Id { get; set; }

        public string Data { get; set; }

    }
}
