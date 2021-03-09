using KeraLua;
using MasterData;
using MasterData.Table;
using System;
using System.IO;
using System.Linq;

namespace TestServer.Model
{
    public class Skill
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

        public Life Owner { get; private set; }
        public IListener Listener { get; private set; }

        public virtual MasterData.Table.Skill Master => MasterTable.From<TableSkill>()[Case];
        public virtual MasterData.Table.SkillProperty Property => MasterTable.From<TableSkillProperty>()[Case].FirstOrDefault(x => x.Level == Level);

        public Skill(Life owner, string id, int level = 1, IListener listener = null)
        {
            Owner = owner;
            Case = id;
            Level = level;
            Listener = listener;
        }

        private void ExecuteActive()
        {
            if (Master.Type != NetworkShared.SkillType.Active)
                return;

            if (Owner.Map == null)
                return;

            var lua = string.IsNullOrEmpty(Master.Script) ? null : Static.Get();
            if(lua != null)
            {
                if (File.Exists(Master.Script) == false)
                    throw new Exception($"{Master.Script} : script not found");

                lua.DoFile(Master.Script);
            }

            lua?.GetGlobal("on_init");
            lua?.Resume(0);

            var targets = Owner.Map.Nears(Owner.Position, Property.Bound);

            switch (Property.Target)
            {
                case NetworkShared.Target.Ally:
                    {
                        targets = targets.Where(x => x.Type == Owner.Type).ToList();
                    }
                    break;

                case NetworkShared.Target.Enemy:
                    {
                        var type = Owner.Type == NetworkShared.ObjectType.Character ?
                            NetworkShared.ObjectType.Mob : NetworkShared.ObjectType.Character;
                        targets = targets.Where(x => x.Type == type).ToList();
                    }
                    break;
            }

            if (Property.TargetCount != null)
                targets = targets.Take(Property.TargetCount.Value).ToList();
        }

        private void ExecuteBuff()
        {
            if (Master.Type != NetworkShared.SkillType.Buff)
                return;
        }

        public void Execute()
        {
            switch (Master.Type)
            {
                case NetworkShared.SkillType.Active:
                    ExecuteActive();
                    break;

                case NetworkShared.SkillType.Buff:
                    ExecuteBuff();
                    break;

                default:
                    break;
            }
        }
    }

    public class Buff : Skill
    {
        public new interface IListener : Skill.IListener
        {
            public void OnStackChanged(Buff buff);
        }

        public Life Caster { get; set; }

        public DateTime ActiveTime { get; set; }

        public new IListener Listener => base.Listener as IListener;

        private int _stack = 0;
        public int Stack
        {
            get => _stack;
            set
            {
                _stack = value;
                ActiveTime = DateTime.Now;
                Listener?.OnStackChanged(this);
            }
        }

        public MasterData.Table.Buff BuffProperty => MasterTable.From<TableBuff>()[Case].FirstOrDefault(x => x.Level == Level);

        public Buff(Life owner, string id, int level = 1, IListener listener = null) : base(owner, id, level, listener)
        {
            
        }
    }

    public class Passive : Skill
    {
        public MasterData.Table.Passive PassiveProperty => MasterTable.From<TablePassive>()[Case].FirstOrDefault(x => x.Level == Level);

        public Passive(Life owner, string id, int level = 1) : base(owner, id, level)
        {
        }
    }
}
