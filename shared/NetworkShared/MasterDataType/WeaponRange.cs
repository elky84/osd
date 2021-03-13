// Generated code. DO NOT MODIFY DIRECTLY

using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MasterData.Common
{
    public partial class WeaponRange
    {
        [NetworkShared.Util.Table.Key]
        public WeaponType WeaponType { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

    }
}
