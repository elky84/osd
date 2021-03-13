// Generated code. DO NOT MODIFY DIRECTLY

using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MasterData.Common
{
    public partial class Consume
    {
        [NetworkShared.Util.Table.Key]
        public string Group { get; set; }

        public string Item { get; set; }

        public int Count { get; set; }

    }
}
