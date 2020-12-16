using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;

namespace MasterData.Table
{
    public class ConsumeOption
    {
        [Key]
        public string Id { get; set; }
        public int HealHP { get; set; }
        public int HealMP { get; set; }
    }
}