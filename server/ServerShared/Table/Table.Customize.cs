using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterData.Table
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

    public partial class TableReward : BaseDict<string, Reward>
    {
        public IEnumerable<Reward> GroupBy(string groupName)
        {
            return this.Values.Where(x => x.Group == groupName);
        }

        public Reward Random(string groupName)
        {
            var candidates = GroupBy(groupName).ToList();
            var value = new Random().Next(0, candidates.Sum(x => x.Weight));
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

    public partial class TableMob : BaseDict<string, Mob>
    { 
    }
}