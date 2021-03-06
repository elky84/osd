using System;
using System.Collections.Generic;
using TestServer.Model;

namespace TestServer.Container
{
    public class ActiveContainer
    {
        private Dictionary<int, Model.Skill> _skills = new Dictionary<int, Model.Skill>();

        public Life Owner { get; private set; }

        public ActiveContainer(Life owner)
        {
            Owner = owner;
        }

        public Model.Skill Set(int slot, Model.Skill skill)
        {
            if (_skills.TryGetValue(slot, out var exists))
                _skills.Remove(slot);

            skill.Owner = Owner;
            _skills.Add(slot, skill);
            return exists;
        }
    }

    public class BuffContainer
    {
        private Dictionary<string, Model.Buff> _buffs = new Dictionary<string, Model.Buff>();

        public Life Owner { get; private set; }

        public BuffContainer(Character owner)
        {
            Owner = owner;
        }

        public Model.Buff Stack(Model.Buff buff)
        {
            var newBuff = new Model.Buff(buff.Owner, buff.Case, buff.Level)
            {
                ActiveTime = DateTime.Now
            };

            if (_buffs.TryGetValue(buff.Case, out var exists))
            {
                newBuff.Stack = exists.Stack + 1;
            }

            return newBuff;
        }
    }

    public class PassiveContainer
    {
        private Dictionary<string, Model.Passive> _passives = new Dictionary<string, Model.Passive>();

        public Character Owner { get; private set; }

        public PassiveContainer(Character owner)
        {
            Owner = owner;
        }

        public Model.Passive Set(Model.Passive passive)
        {
            if (_passives.TryGetValue(passive.Case, out var exists))
            {
                if (exists.Level > passive.Level)
                    return exists;
            }

            _passives[passive.Case] = passive;
            return passive;
        }
    }

    public class SkillContainer
    {
        public Life Owner { get; private set; }

        public ActiveContainer Actives { get; private set; }

        public BuffContainer Buffs { get; private set; }

        public PassiveContainer Passives { get; private set; }


        public SkillContainer(Character owner)
        {
            Owner = owner;
            Actives = new ActiveContainer(owner);
            Buffs = new BuffContainer(owner);
            Passives = new PassiveContainer(owner);
        }
    }
}
