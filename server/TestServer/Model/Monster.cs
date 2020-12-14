using KeraLua;
using NetworkShared;
using System;

namespace TestServer.Model
{
    public class Monster : Life
    {
        public bool Attackable { get; set; } = true;

        public override ObjectType Type => ObjectType.Mob;

        public static int BuiltInAttackable(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var mob = lua.ToLuable<Monster>(1);

            lua.PushBoolean(mob.Attackable);
            return 1;
        }
    }
}
