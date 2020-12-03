using KeraLua;
using System;
using System.Drawing;

namespace TestServer.Model
{
    public class Object : ILuable
    {
        public Point Position { get; set; } = new Point();
        public Map Map { get; set; }

        public static int BuiltinPosition(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var obj = lua.ToLuable<Object>(1);

            lua.PushInteger(obj.Position.X);
            lua.PushInteger(obj.Position.Y);
            return 2;
        }

        public static int BuiltinMap(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var obj = lua.ToLuable<Object>(1);

            lua.PushLuable<Map>(obj.Map);
            return 1;
        }
    }
}
