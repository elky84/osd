using KeraLua;
using System;

namespace TestServer.Model
{
    public class Monster : Life
    {
        public bool Attackable { get; set; } = true;

        public static int BuiltInAttackable(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var mob = lua.ToLuable<Monster>(1);

            lua.PushBoolean(mob.Attackable);
            return 1;
        }
    }
}
