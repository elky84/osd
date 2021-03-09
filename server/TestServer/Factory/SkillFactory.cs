using MasterData;
using MasterData.Table;

namespace TestServer.Factory
{
    public class SkillFactory
    {
        public static Model.Skill Create(string id, int level = 1)
        {
            var master = MasterTable.From<TableSkill>()[id];
            if (master == null)
                return null;

            switch (master.Type)
            {
                case NetworkShared.SkillType.Passive:
                    return new Model.Passive(id, level);

                case NetworkShared.SkillType.Buff:
                    return new Model.Buff(id, level);

                default:
                    return new Model.Skill(id, level);
            }
        }
    }
}
