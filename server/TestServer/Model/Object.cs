using KeraLua;
using System;
using System.Drawing;

namespace TestServer.Model
{
    public class Object : ILuable
    {
        public Position Position { get; set; } = new Position();
        public Map Map { get; set; }

        public static int BuiltinPosition(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var obj = lua.ToLuable<Object>(1);

            //TODO double, int 체크
            lua.PushInteger((int)obj.Position.X);
            lua.PushInteger((int)obj.Position.Y);
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
