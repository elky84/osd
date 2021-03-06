using NetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestServer.Container
{
    public class StatContainer
    {
        public Dictionary<StatType, int> Base { get; private set; } = new Dictionary<StatType, int>();
        public Dictionary<StatType, int> Additional { get; private set; } = new Dictionary<StatType, int>();
        public Dictionary<StatType, int> Max
        {
            get
            {
                var current = new Dictionary<StatType, int>();
                foreach (var statType in Enum.GetValues(typeof(StatType)).Cast<StatType>())
                    current[statType] = Base[statType] + Additional[statType];

                return current;
            }
        }

        public StatContainer()
        {
            foreach (var statType in Enum.GetValues(typeof(StatType)).Cast<StatType>())
            {
                Base.Add(statType, 0);
                Additional.Add(statType, 0);
            }
        }
    }

    public static class StatContainerExtension
    {
        public static void Set(this Dictionary<StatType, int> stats, Dictionary<StatType, int> values)
        {
            foreach (var (statType, value) in values)
            {
                stats[statType] = value;
            }
        }
    }
}
