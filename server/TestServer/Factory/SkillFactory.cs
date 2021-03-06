using MasterData;
using MasterData.Table;
using System;
using TestServer.Model;

namespace TestServer.Factory
{
    public class SkillFactory
    {
        public static Model.Skill Create(Character owner, string id, int level = 1)
        {
            var master = MasterTable.From<TableSkill>()[id];
            if (master == null)
                return null;

            switch (master.Type)
            {
                case NetworkShared.SkillType.Passive:
                    return new Model.Passive(owner, id, level);

                case NetworkShared.SkillType.Buff:
                    return new Model.Buff(owner, id, level);

                default:
                    return new Model.Active(owner, id, level);
            }
        }
    }
}
