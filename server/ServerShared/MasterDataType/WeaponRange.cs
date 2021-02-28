using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json.Converters;

namespace MasterData.Table
{
    public partial class WeaponRange
    {
        [Key]
        [JsonConverter(typeof(StringEnumConverter))]
        public WeaponType WeaponType { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

    }
}
