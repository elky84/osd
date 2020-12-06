using KeraLua;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TestServer.Model
{
    public partial class Map : ILuable
    {
        private int _currentSequence = 0;

        public string Name { get; private set; }
        public Size Size { get; private set; }
        public SectorContainer Sectors { get; private set; }
        public Dictionary<int, Object> Objects { get; private set; } = new Dictionary<int, Object>();
        public int? NextSequence
        {
            get
            {
                for (int i = _currentSequence; i < int.MaxValue; i++)
                {
                    if (Objects.ContainsKey(i))
                        continue;

                    _currentSequence = i + 1;
                    return i;
                }

                for (int i = 0; i < _currentSequence; i++)
                {
                    if (Objects.ContainsKey(i))
                        continue;

                    _currentSequence = i + 1;
                    return i;
                }

                return null;
            }
        }

        public static int BuiltinName(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var map = lua.ToLuable<Map>(1);

            lua.PushString(map.Name);
            return 1;
        }

        public static int BuiltinSize(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var map = lua.ToLuable<Map>(1);

            lua.PushInteger(map.Size.Width);
            lua.PushInteger(map.Size.Height);
            return 2;
        }

        public static int BuiltinObjects(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var map = lua.ToLuable<Map>(1);

            lua.NewTable();
            foreach (var (obj, i) in map.Objects.Select((x, i) => (x, i)))
            {
                lua.PushLuable(obj.Value);
                lua.RawSetInteger(-2, i + 1);
            }

            return 1;
        }

        public Map(string name, Size size)
        {
            if (string.IsNullOrEmpty(name))
                throw new Exception("map name cannot be null or empty.");

            if (size.IsEmpty)
                throw new Exception($"map size cannot be empty : {name}.");

            Name = name;
            Size = size;

#if DEBUG
            Sectors = new SectorContainer(this, new Size(8, 8));
#else
            Sectors = new SectorContainer(this, new Size(64, 64));
#endif
        }

        public Sector Add(Object obj)
        {
            var sequence = NextSequence ??
                throw new Exception("cannot get next sequence.");

            Objects.Add(sequence, obj);
            obj.Map = this;
            obj.Sequence = sequence;
            obj.Sector = Sectors.Add(obj);
            return obj.Sector;
        }

        public Sector Add(Object obj, Position position)
        {
            obj.Position = position;
            return Add(obj);
        }

        public Sector Remove(Object obj)
        {
            if (obj.Sequence.HasValue == false)
                return null;

            obj.Map = null;
            Objects.Remove(obj.Sequence.Value);
            obj.Sequence = null;
            return Sectors.Remove(obj);
        }

        public Sector Update(Object obj)
        {
            if (obj.Sequence.HasValue == false || Objects.ContainsKey(obj.Sequence.Value) == false)
                return null;

            if (obj.Sector == null)
                return null;

            return Sectors.Add(obj);
        }
    }
}
