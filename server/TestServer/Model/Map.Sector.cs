using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TestServer.Model
{
    public partial class Map
    {
        public class Sector : IDictionary<int, Object>
        {
            private Dictionary<int, Object> _objects = new Dictionary<int, Object>();
            private bool _activated = false;
            private Action<Sector> _stateChangedEvent;

            public Object this[int key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public uint Id { get; private set; }

            public IEnumerable<Object> Objects => _objects.Values;

            public IEnumerable<Character> Characters => _objects.Select(x => x.Value as Character).Where(x => x != null);

            public bool Activated
            {
                get => _activated;
                set
                {
                    if (_activated != value)
                    {
                        _stateChangedEvent.Invoke(this);
                        _activated = value;
                    }
                }
            }

            public int Count => _objects.Count;

            public bool IsReadOnly => false;

            public ICollection<int> Keys => _objects.Keys;

            public ICollection<Object> Values => _objects.Values;

            public void Add(int key, Object value)
            {
                _objects.Add(key, value);
                if (value is Character)
                    Activated = true;
            }

            public void Add(KeyValuePair<int, Object> item) => _objects.Add(item.Key, item.Value);

            public void Clear()
            {
                _objects.Clear();
                Activated = false;
            }

            public bool Contains(KeyValuePair<int, Object> item) => _objects.ContainsKey(item.Key);

            public bool ContainsKey(int key) => _objects.ContainsKey(key);

            public Sector(uint id, Action<Sector> stateChangedEvent)
            {
                Id = id;
                _stateChangedEvent = stateChangedEvent;
            }

            private void Update() => Activated = (Characters.Count() > 0);

            public bool TryGetValue(int key, [MaybeNullWhen(false)] out Object value) => _objects.TryGetValue(key, out value);

            public IEnumerator<KeyValuePair<int, Object>> GetEnumerator() => _objects.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _objects.GetEnumerator();

            public bool Remove(int key)
            {
                if (_objects.Remove(key) == false)
                    return false;
                
                Update();
                return true;
            }

            public void CopyTo(KeyValuePair<int, Object>[] array, int arrayIndex) => throw new NotImplementedException();

            public bool Remove(KeyValuePair<int, Object> item)
            {
                return Remove(item.Key);
            }
        }
    }
}
