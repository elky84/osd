using KeraLua;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TestServer.Model
{
    public partial class Map : ILuable
    {
        public string Name { get; private set; }
        public Size Size { get; private set; }
        public SectorContainer Sectors { get; private set; }
        public IEnumerable<Object> Objects => Sectors.SelectMany(x => x.Objects);

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
                lua.PushLuable(obj);
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

            Sectors = new SectorContainer(this, new Size(64, 64));
        }

        public Sector Add(Object obj)
        {
            obj.Map = this;
            return Sectors.Add(obj);
        }

        public Sector Add(Object obj, Point position)
        {
            obj.Position = position;
            return Add(obj);
        }

        public Sector Remove(Object obj)
        {
            obj.Map = null;
            return Sectors.Remove(obj);
        }
    }
}
