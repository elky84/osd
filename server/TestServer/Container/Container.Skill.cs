using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TestServer.Model;

namespace TestServer.Container
{
    public class SkillCollection : IEnumerable<Skill>
    {
        private List<Skill> _skill = new List<Skill>();

        public Character Owner { get; private set; }

        public IEnumerator<Skill> GetEnumerator() => _skill.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _skill.GetEnumerator();

        public SkillCollection(Character owner)
        {
            Owner = owner;
        }

        public void Set(Skill skill, Skill.IListener listener)
        {
            var exists = _skill.FirstOrDefault(x => x.Case == skill.Case);
            if (exists.Level < skill.Level)
                _skill.Remove(exists);

            _skill.Add(new Skill(Owner, skill.Case, skill.Level, listener));
        }

        public void Set(string skillId, Skill.IListener listener)
        {
            _skill.Add(new Skill(Owner, skillId, listener: listener));
        }
    }

    public class BuffCollection : IEnumerable<Buff>
    {
        private List<Buff> _buffs;

        public IEnumerator<Buff> GetEnumerator() => _buffs.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _buffs.GetEnumerator();

        public void Add(Buff buff, Buff.IListener listener)
        {
            var exists = _buffs.FirstOrDefault(x => x.Case == buff.Case) ??
                new Buff(buff.Owner, buff.Case, buff.Level, listener);

            if (exists.Level < buff.Level)
            {
                exists.Owner = buff.Owner;
                exists.Level = buff.Level;
            }

            exists.Stack++;
        }
    }
}
