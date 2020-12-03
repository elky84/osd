using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TestServer.Model
{
    public partial class Map
    {
        public class Sector : IList<Object>
        {
            private List<Object> _objects = new List<Object>();
            private bool _activated = false;
            private Action<Sector> _stateChangedEvent;

            public uint Id { get; private set; }

            public IEnumerable<Object> Objects => _objects;

            public IEnumerable<Character> Characters => _objects.Select(x => x as Character).Where(x => x != null);

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

            Object IList<Object>.this[int index]
            {
                get => _objects[index];
                set { }
            }

            public Object this[int index] => _objects[index];

            public Sector(uint id, Action<Sector> stateChangedEvent)
            {
                Id = id;
                _stateChangedEvent = stateChangedEvent;
            }

            private void Update() => Activated = (Characters.Count() > 0);

            public int IndexOf(Object item) => _objects.IndexOf(item);

            public void Insert(int index, Object item) => _objects.Insert(index, item);

            public void RemoveAt(int index)
            { 
                 _objects.RemoveAt(index);
                Update();
            }

            public void Add(Object item)
            {
                _objects.Add(item);
                if (item is Character)
                    Activated = true;
            }

            public void Clear()
            {
                Activated = false;
            }

            public bool Contains(Object item) => _objects.Contains(item);

            public void CopyTo(Object[] array, int arrayIndex) => _objects.CopyTo(array, arrayIndex);

            public bool Remove(Object item)
            {
                if (_objects.Remove(item) == false)
                    return false;
                    
                Update();
                return true;
            }

            public IEnumerator<Object> GetEnumerator() => _objects.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _objects.GetEnumerator();
        }
    }
}
