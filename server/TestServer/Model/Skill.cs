using MasterData;
using MasterData.Table;
using NetworkShared;
using System;
using System.Linq;

namespace TestServer.Model
{
    public abstract class Skill
    {
        public interface IListener
        {
            public void OnLevelUp(Skill skill);
        }

        public string Case { get; set; }

        private int _level;
        public int Level
        {
            get => _level;
            set
            {
                _level = value;
                Listener?.OnLevelUp(this);
            }
        }

        public Life Owner { get; set; }

        public IListener Listener { get; set; }

        public abstract SkillType Type { get; }

        public virtual MasterData.Table.Skill Master => MasterTable.From<TableSkill>()[Case];
        public virtual MasterData.Table.SkillProperty Property => MasterTable.From<TableSkillProperty>()[Case].FirstOrDefault(x => x.Level == Level);

        public Skill()
        { }

        public Skill(Life owner, string id, int level = 1)
        {
            Owner = owner;
            Case = id;
            Level = level;
        }
    }

    public class Active : Skill
    {
        public override SkillType Type => SkillType.Active;

        public Active(Life owner, string id, int level = 1) : base(owner, id, level)
        {
        }
    }

    public class Passive : Skill
    {
        public override SkillType Type => SkillType.Passive;

        public int Stack { get; set; }

        public virtual MasterData.Table.Passive PassiveProperty => MasterTable.From<TablePassive>()[Case].FirstOrDefault(x => x.Level == Level);

        public Passive()
        { }

        public Passive(Life owner, string id, int level = 1) : base(owner, id, level)
        { 
        }
    }

    public class Buff : Passive
    {
        public override SkillType Type => SkillType.Buff;

        public virtual DateTime ActiveTime { get; set; }

        public virtual MasterData.Table.Buff BuffProperty => MasterTable.From<TableBuff>()[Case].FirstOrDefault(x => x.Level == Level);

        public Buff()
        { }

        public Buff(Life owner, string id, int level = 1) : base(owner, id, level)
        {
            
        }
    }
}
