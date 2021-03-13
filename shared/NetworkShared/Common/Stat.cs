using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NetworkShared.Common
{
    public class Stat : IEnumerable<KeyValuePair<StatType, int>>
    {
        private Dictionary<StatType, int> _buffer = new Dictionary<StatType, int>();

        public Action<Stat> OnStatChanged { get; set; }

        public Stat()
        {
            foreach (var statType in Enum.GetValues(typeof(StatType)).Cast<StatType>())
                _buffer.Add(statType, default);
        }

        public Stat(MasterData.Common.Stat stat) : base()
        {
            _buffer = new Dictionary<NetworkShared.StatType, int>
            {
                { NetworkShared.StatType.HP, stat.Hp },
                { NetworkShared.StatType.MP, stat.Mp },
                { NetworkShared.StatType.Defence, stat.Defence },
                { NetworkShared.StatType.PhysicalDamage, stat.PhysicalDamage },
                { NetworkShared.StatType.MagicalDamage, stat.MagicalDamage },
                { NetworkShared.StatType.AttackSpeed, stat.AttackSpeed },
                { NetworkShared.StatType.Critical, stat.Critical },
                { NetworkShared.StatType.CriticalDamage, stat.CriticalDamage }
            };
        }

        public IEnumerator<KeyValuePair<StatType, int>> GetEnumerator() => _buffer.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _buffer.GetEnumerator();

        public void Set(Stat values)
        {
            foreach (var pair in values)
            {
                _buffer[pair.Key] = pair.Value;
            }
            OnStatChanged?.Invoke(this);
        }

        public int this[StatType type]
        {
            get => _buffer[type];
            set
            {
                _buffer[type] = value;
                OnStatChanged?.Invoke(this);
            }
        }

        public static Stat operator +(Stat stat1, Stat stat2)
        {
            var result = new Stat();
            foreach (var statType in Enum.GetValues(typeof(StatType)).Cast<StatType>())
                result[statType] = stat1[statType] + stat2[statType];

            return result;
        }

        public static Stat operator -(Stat stat1, Stat stat2)
        {
            var result = new Stat();
            foreach (var statType in Enum.GetValues(typeof(StatType)).Cast<StatType>())
                result[statType] = stat1[statType] - stat2[statType];

            return result;
        }
    }

    public class StatContainer
    {
        public Stat Init { get; private set; } = new Stat();
        public Stat Level { get; private set; } = new Stat();
        public Stat Equipment { get; private set; } = new Stat();
        public Stat Buff { get; private set; } = new Stat();
        public Stat Stored { get; private set; } = new Stat();

        public Stat Base { get; private set; } = new Stat();
        public Stat Additional { get; private set; } = new Stat();
        public Stat Max { get; private set; } = new Stat();

        public StatContainer()
        {
            Init.OnStatChanged = this.OnBaseStatChanged;
            Level.OnStatChanged = this.OnBaseStatChanged;
            Stored.OnStatChanged = this.OnBaseStatChanged;

            Equipment.OnStatChanged = this.OnAdditionalStatChanged;
            Buff.OnStatChanged = this.OnAdditionalStatChanged;

            Base.OnStatChanged = this.OnMaxStatChanged;
            Additional.OnStatChanged = this.OnMaxStatChanged;
        }

        private void OnMaxStatChanged(Stat obj)
        {
            Max.Set(Base + Additional);
        }

        private void OnBaseStatChanged(Stat obj)
        {
            Base.Set(Init + Level);
        }

        private void OnAdditionalStatChanged(Stat obj)
        {
            Additional.Set(Equipment + Buff);
        }

        public double PhysicalDamage(Stat stat)
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var critical = Math.Min(1.0, Max[StatType.Critical] / 1000000.0) > random.NextDouble();
            if (critical)
            {
                return Max[StatType.PhysicalDamage] + (Max[StatType.PhysicalDamage] * Max[StatType.CriticalDamage] / 100.0) - stat[StatType.Defence];
            }
            else
            {
                return Max[StatType.PhysicalDamage] - stat[StatType.Defence];
            }
        }

        public double MagicalDamage(Stat stat)
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var critical = Math.Min(1.0, Max[StatType.Critical] / 1000000.0) > random.NextDouble();
            if (critical)
            {
                return Max[StatType.MagicalDamage] + (Max[StatType.MagicalDamage] * Max[StatType.MagicalDamage] / 100.0) - stat[StatType.Defence];
            }
            else
            {
                return Max[StatType.MagicalDamage] - stat[StatType.Defence];
            }
        }
    }
}
