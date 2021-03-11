using KeraLua;
using MasterData;
using MasterData.Table;
using System;
using System.Collections.Generic;
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

        public Life Owner { get; set; }
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

        private List<Object> Targets
        {
            get
            {
                var targets = new List<Object>();
                switch (Property.Target)
                {
                    case NetworkShared.Target.Ally:
                        {
                            targets = Owner.Map.Nears(Owner.Position, Property.Bound).Where(x => x.Type == Owner.Type).ToList();
                        }
                        break;

                    case NetworkShared.Target.Enemy:
                        {
                            var type = Owner.Type == NetworkShared.ObjectType.Character ?
                                NetworkShared.ObjectType.Mob : NetworkShared.ObjectType.Character;
                            targets = Owner.Map.Nears(Owner.Position, Property.Bound).Where(x => x.Type == type).ToList();
                        }
                        break;

                    case NetworkShared.Target.Self:
                        targets.Add(Owner);
                        break;
                }

                if (Property.TargetCount != null)
                    targets = targets.Take(Property.TargetCount.Value).ToList();

                return targets;
            }
        }

        private List<Object> ExecuteActive()
        {
            if (Owner.Map == null)
                return new List<Object>();

            var targets = Targets;
            if (Property.HPRecovery != null)
            {
                targets.ForEach(x =>
                {
                    switch (x.Type)
                    {
                        case NetworkShared.ObjectType.Character:
                            (x as Character).Heal(Property.HPRecovery.Value);
                            break;

                        case NetworkShared.ObjectType.Mob:
                            (x as Mob).Heal(Property.HPRecovery.Value);
                            break;
                    }
                });
            }

            if (Property.MPRecovery != null)
            {
                targets.ForEach(x =>
                {
                    switch (x.Type)
                    {
                        case NetworkShared.ObjectType.Character:
                            (x as Character).Mp += Property.MPRecovery.Value;
                            break;
                    }
                });
            }

            return targets;
        }

        private void ExecuteBuff()
        {
            if (Master.Type != NetworkShared.SkillType.Buff)
                return;

            var targets = ExecuteActive();
            targets.ForEach(x =>
            {
                var life = x as Life;
                if (life == null)
                    return;

                life.Buffs.Add(new Buff(Owner, Case, Level, life));
            });
        }

        public void Execute()
        {
            var lua = string.IsNullOrEmpty(Master.Script) ? null : Static.Get();
            if (lua != null)
            {
                if (File.Exists(Master.Script) == false)
                    throw new Exception($"{Master.Script} : script not found");

                lua.DoFile(Master.Script);
            }

            lua?.GetGlobal("on_init");
            switch (Owner.Type)
            {
                case NetworkShared.ObjectType.Character:
                    lua?.PushLuable(Owner as Character);
                    break;

                case NetworkShared.ObjectType.Mob:
                    lua?.PushLuable(Owner as Mob);
                    break;
            }
            lua?.Resume(1);

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

            lua?.GetGlobal("on_finish");
            switch (Owner.Type)
            {
                case NetworkShared.ObjectType.Character:
                    lua?.PushLuable(Owner as Character);
                    break;

                case NetworkShared.ObjectType.Mob:
                    lua?.PushLuable(Owner as Mob);
                    break;
            }
            lua?.Resume(1);
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
        public DateTime LastIntervalTime { get; set; }

        public new IListener Listener => base.Listener as IListener;

        private int _stack = 0;
        public int Stack
        {
            get => _stack;
            set
            {
                _stack = value;
                ActiveTime = LastIntervalTime = DateTime.Now;
                Listener?.OnStackChanged(this);
            }
        }

        public MasterData.Table.Buff BuffProperty => MasterTable.From<TableBuff>()[Case].FirstOrDefault(x => x.Level == Level);

        public Buff(Life owner, string id, int level = 1, IListener listener = null) : base(owner, id, level, listener)
        {
            
        }

        public void ExecuteTick()
        {
            var now = DateTime.Now;
            var elapsed = (now - LastIntervalTime).TotalMilliseconds;
            if (elapsed < BuffProperty.Interval)
                return;

            // 여기서 틱마다 효과 발생
            LastIntervalTime = now;
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
