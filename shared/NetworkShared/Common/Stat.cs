using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NetworkShared.Common
{
    public class Stat : IEnumerable<KeyValuePair<StatType, int>>
    {
        private Dictionary<StatType, int> _buffer = new Dictionary<StatType, int>();

        public Stat()
        {
            foreach (var statType in Enum.GetValues(typeof(StatType)).Cast<StatType>())
                _buffer.Add(statType, default);
        }

        public IEnumerator<KeyValuePair<StatType, int>> GetEnumerator() => _buffer.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _buffer.GetEnumerator();

        public int this[StatType type]
        {
            get => _buffer[type];
            set => _buffer[type] = value;
        }

        public static Stat operator +(Stat stat1, Stat stat2)
        {
            var result = new Stat();
            foreach (var statType in Enum.GetValues(typeof(StatType)).Cast<StatType>())
                result[statType] = stat1[statType] + stat2[statType];

            return result;
        }
    }

    public class StatContainer
    {
        public Stat Base { get; private set; } = new Stat();
        public Stat Additional { get; private set; } = new Stat();
        public Stat Max => Base + Additional;

        public double Damage(Stat stat)
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
    }

    public static class StatContainerExtension
    {
        public static void Set(this Stat stats, Dictionary<StatType, int> values)
        {
            foreach (var pair in values)
            {
                stats[pair.Key] = pair.Value;
            }
        }
    }
}
