// Generated code. DO NOT MODIFY DIRECTLY

using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MasterData.Server
{
    public partial class Portal : MasterData.Common.Portal
    {
        [NetworkShared.Util.Table.Key]
        public string Id { get; set; }

        public string BeforeMap { get; set; }

        public Point BeforePosition { get; set; }

        public string AfterMap { get; set; }

        public Point AfterPosition { get; set; }

    }
}
