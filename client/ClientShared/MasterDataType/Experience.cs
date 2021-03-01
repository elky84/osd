using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json.Converters;

namespace MasterData.Table
{
    public partial class Experience
    {
        [Key]
        public int Level { get; set; }

        public int Value { get; set; }

    }
}
