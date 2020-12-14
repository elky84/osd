using KeraLua;
using System;

namespace TestServer.Model
{
    public class Object : ILuable
    {
        public string Name { get; set; }
        public Position Position { get; set; } = new Position();
        public Map Map { get; set; }
        public int? Sequence { get; set; }
        public Map.Sector Sector { get; set; }

        public static int BuiltinName(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var obj = lua.ToLuable<Object>(1);

            lua.PushString(obj.Name);
            return 1;
        }

        public static int BuiltinPosition(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var obj = lua.ToLuable<Object>(1);

            lua.PushNumber(obj.Position.X);
            lua.PushNumber(obj.Position.Y);
            return 2;
        }

        public static int BuiltinMap(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var obj = lua.ToLuable<Object>(1);

            lua.PushLuable(obj.Map);
            return 1;
        }
    }
}
