using NetworkShared.Util.Table;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;


namespace MasterData
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string Path { get; private set; }

        public TableAttribute(string path)
        {
            Path = path;
        }
    }

    public abstract class BaseTable
    {
        protected string Path => GetType().GetCustomAttribute<TableAttribute>()?.Path ?? 
            throw new Exception("cannot find TableAttribute.");

        protected BaseTable()
        {
            Load();
        }

        protected abstract void Load();
    }

    public class BaseList<T> : BaseTable, IReadOnlyList<T> where T : class
    {
        private List<T> _values = new List<T>();

        public T this[int index]
        {
            get
            {
                if (index > _values.Count - 1)
                    return null;

                return _values[index];
            }
        }

        public int Count => _values.Count;

        public IEnumerator<T> GetEnumerator() => _values.GetEnumerator();

        protected override void Load()
        {
            _values = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(Path));
        }

        IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
    }

    public class BaseDict<K, T> : BaseTable, IReadOnlyDictionary<K, T> where T : class
    {
        private Dictionary<K, T> _dictionary = new Dictionary<K, T>();

        public T this[K key]
        {
            get
            {
                if (_dictionary.TryGetValue(key, out var value) == false)
                    return null;

                return value;
            }
        }

        public IEnumerable<K> Keys => _dictionary.Keys;

        public IEnumerable<T> Values => _dictionary.Values;

        public int Count => _dictionary.Count;

        public bool ContainsKey(K key) => _dictionary.ContainsKey(key);

        public IEnumerator<KeyValuePair<K, T>> GetEnumerator() => _dictionary.GetEnumerator();

        public bool TryGetValue(K key, out T value) => _dictionary.TryGetValue(key, out value);

        protected override void Load()
        {
            var property = typeof(T).GetProperties().FirstOrDefault(x => x.GetCustomAttribute<KeyAttribute>() != null);
            
            _dictionary = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(Path))
                .ToDictionary(x => (K)property.GetValue(x), x => x);
        }

        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();
    }

    public static class MasterTable
    {
        private static Dictionary<Type, BaseTable> _loadedTableDict = new Dictionary<Type, BaseTable>();

        static MasterTable()
        {
            //Load();
        }

        public static void Load(string assemblyName = null)
        {
            var assembly = string.IsNullOrEmpty(assemblyName) ?
                Assembly.GetEntryAssembly() :
                AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == assemblyName);

            _loadedTableDict = assembly.GetTypes()
                .Where(x => x.IsSubclassOf(typeof(BaseTable)))
                .ToDictionary(x => x, x => Activator.CreateInstance(x) as BaseTable);
        }

        public static T From<T>() where T : BaseTable
        {
            if (_loadedTableDict.TryGetValue(typeof(T), out var value) == false)
                return null;

            return value as T;
        }
    }
}
