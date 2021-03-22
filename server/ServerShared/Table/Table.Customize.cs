using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterData.Server
{
    public partial class Portal
    {
        public static implicit operator FlatBuffers.Protocol.Response.Portal.Model(Portal p) => new FlatBuffers.Protocol.Response.Portal.Model(p.BeforePosition, p.AfterMap);
    }


    public partial class TablePortal : BaseDict<string, Portal>
    {
        public List<Portal> Nears(string mapName)
        {
            return this.Values.Where(x => x.BeforeMap == mapName).ToList();
        }
    }

    public partial class TableReward : BaseDict<string, List<Reward>>
    {
        public Reward Random(string groupName)
        {
            var random = new Random();
            var candidates = this[groupName];
            if (candidates == null)
                return null;

            var weights = candidates.Sum(x => x.Weight);
            var value = random.Next(0, weights);
            var current = 0;

            foreach (var reward in candidates)
            {
                current += reward.Weight;
                if (value < current)
                    return reward;
            }

            return null;
        }
    }

    public partial class Stat
    {
        public NetworkShared.Common.Stat ToStat()
        {
            return new NetworkShared.Common.Stat(this);
        }
    }
}