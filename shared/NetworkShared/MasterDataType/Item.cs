// Generated code. DO NOT MODIFY DIRECTLY

using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MasterData.Common
{
    public partial class Item
    {
        [NetworkShared.Util.Table.Key]
        public string Id { get; set; }

        public ItemType Type { get; set; }

        public int? Stack { get; set; }

    }
}