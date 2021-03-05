using MasterData;
using MasterData.Table;
using System;
using TestServer.Model;

namespace TestServer.Factory
{
    public class SkillFactory
    {
        public static Model.Skill Create(Character owner, string id, int level = 1, DateTime? activeTime = null)
        {
            var master = MasterTable.From<TableSkill>()[id];
            if (master == null)
                return null;

            switch (master.Type)
            {
                case NetworkShared.SkillType.Passive:
                    return new Model.Passive(0, id, level);

                case NetworkShared.SkillType.Buff:
                    return new Model.Buff(0, id, level, activeTime);

                default:
                    return new Model.Active(0, id, level);
            }
        }
    }
}
