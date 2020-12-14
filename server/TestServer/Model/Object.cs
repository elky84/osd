using KeraLua;
using NetworkShared;
using NetworkShared.Types;
using System;

namespace TestServer.Model
{
    public abstract class Object : ILuable
    {
        public interface IListener
        {
            public void OnLeave(Object obj);
            public void OnEnter(Object obj);
            public void OnSectorChanged(Object obj);
        }

        public abstract ObjectType Type { get; }

        public IListener Listener { get; set; }

        public string Name { get; set; }
        public Point Position { get; set; } = new Point();
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
