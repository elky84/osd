using KeraLua;
using System;
using System.Drawing;

namespace TestServer.Model
{
    public class Object : ILuable
    {
        public Point Position { get; set; } = new Point();

        public static int BuiltinPosition(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var obj = lua.ToLuable<Object>(1);

            lua.PushInteger(obj.Position.X);
            lua.PushInteger(obj.Position.Y);
            return 2;
        }
    }
}
