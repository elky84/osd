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

        public Skill this[int index]
        {
            get
            {
                try
                {
                    return _skill[index];
                }
                catch (IndexOutOfRangeException)
                {
                    return null;
                }
            }
        }

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
        private Dictionary<string, Buff> _buffs = new Dictionary<string, Buff>();

        public IEnumerator<Buff> GetEnumerator() => _buffs.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _buffs.Values.GetEnumerator();

        public void Add(Buff buff)
        {
            if (_buffs.TryGetValue(buff.Case, out var exists) == false)
            {
                exists = new Buff(buff.Owner, buff.Case, buff.Level, buff.Listener);
                _buffs.Add(buff.Case, exists);
            }

            if (exists.Level < buff.Level)
            {
                exists.Owner = buff.Owner;
                exists.Level = buff.Level;
            }

            exists.Stack++;
        }

        public void Remove(Buff buff)
        {
            _buffs.Remove(buff.Case);
            buff.Listener?.OnBuffFinish(buff);
        }
    }
}
