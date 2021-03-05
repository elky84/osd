using MasterData;
using MasterData.Table;
using NetworkShared;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace TestServer.Model
{
    public class Skill
    {
        public interface IListener
        {
            public void OnLevelUp(Skill skill);
        }

        public long UserId { get; set; }
        public string Case { get; set; }

        private int _level = 1;
        public int Level
        {
            get => _level;
            set
            {
                _level = value;
                Listener?.OnLevelUp(this);
            }
        }


        public virtual int Stack { get; set; }


        public virtual DateTime? ActiveTime { get; set; }

        [JsonIgnore]
        public virtual SkillType Type { get; }
        [JsonIgnore]
        public IListener Listener { get; set; }

        [JsonIgnore]
        public virtual Character Owner { get; set; }
        [JsonIgnore]
        public virtual MasterData.Table.Skill Master => MasterTable.From<TableSkill>()[Case];
        [JsonIgnore]
        public virtual MasterData.Table.SkillProperty Property => MasterTable.From<TableSkillProperty>()[Case].FirstOrDefault(x => x.Level == Level);
        [JsonIgnore]
        public virtual MasterData.Table.Passive PassiveProperty => MasterTable.From<TablePassive>()[Case].FirstOrDefault(x => x.Level == Level);
        [JsonIgnore]
        public virtual MasterData.Table.Buff BuffProperty => MasterTable.From<TableBuff>()[Case].FirstOrDefault(x => x.Level == Level);

        public Skill()
        { }

        public Skill(long userId, string id, int level = 1)
        {
            UserId = userId;
            Case = id;
            Level = level;
        }
    }

    public class Active : Skill
    {
        public override SkillType Type => SkillType.Active;

        public override DateTime? ActiveTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int Stack { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override MasterData.Table.Passive PassiveProperty => throw new NotImplementedException();
        public override MasterData.Table.Buff BuffProperty => throw new NotImplementedException();

        public Active()
        { }

        public Active(long userId, string id, int level = 1) : base(userId, id, level)
        {
        }
    }

    public class Passive : Skill
    {
        public override SkillType Type => SkillType.Passive;

        public override DateTime? ActiveTime { get => DateTime.MinValue; set => throw new NotImplementedException(); }
        public override MasterData.Table.Buff BuffProperty => throw new NotImplementedException();

        public Passive()
        { }

        public Passive(long userId, string id, int level = 1) : base(userId, id, level)
        { 
        }
    }

    public class Buff : Passive
    {
        public override SkillType Type => SkillType.Buff;

        public override MasterData.Table.Passive PassiveProperty => throw new NotImplementedException();

        public Buff()
        { }

        public Buff(long userId, string id, int level = 1, DateTime? activeTime = null) : base(userId, id, level)
        {
            ActiveTime = activeTime;
        }
    }
}
