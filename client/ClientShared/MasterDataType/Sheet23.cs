using System;
using System.Collections.Generic;
using NetworkShared.Util.Table;
using Newtonsoft.Json;
using NetworkShared;
using NetworkShared.Types;

namespace MasterData.Table
{
    public class Sheet23
    {
        [Key]
        public string id { get; set; }
        public double? value { get; set; }
        public int cshyeon { get; set; }
        public string relation { get; set; }
    }
}