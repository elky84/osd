using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;

namespace MasterData.Table
{
    public partial class Collision
    {
        [Key]
        public string Id { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

    }
}
