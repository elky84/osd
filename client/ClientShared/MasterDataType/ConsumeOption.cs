using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json.Converters;

namespace MasterData.Table
{
    public partial class ConsumeOption
    {
        [Key]
        public string Id { get; set; }

        public int HPRecovery { get; set; }

        public int MPRecovery { get; set; }

    }
}
