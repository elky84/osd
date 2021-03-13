// Generated code. DO NOT MODIFY DIRECTLY

using NetworkShared;
using NetworkShared.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MasterData.Common
{
    public partial class Skill
    {
        [NetworkShared.Util.Table.Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public SkillType Type { get; set; }

    }
}
