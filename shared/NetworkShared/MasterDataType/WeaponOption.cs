// Generated code. DO NOT MODIFY DIRECTLY

using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MasterData.Common
{
    public partial class WeaponOption
    {
        [NetworkShared.Util.Table.Key]
        public string Id { get; set; }

        public WeaponType Type { get; set; }

    }
}