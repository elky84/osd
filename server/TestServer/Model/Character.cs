using DotNetty.Transport.Channels;
using KeraLua;
using System;

namespace TestServer.Model
{
    public class Character : Life
    {
        public IChannelHandlerContext Context { get; set; }
        public Lua DialogThread { get; set; }

        public int Damage { get; set; } = 30;

        public static int BuiltinDamage(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var character = lua.ToLuable<Character>(1);
            lua.PushInteger(character.Damage);
            return 1;
        }

        public static int BuiltinYield(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var character = lua.ToLuable<Character>(1);
            character.Damage = 10;
            return lua.Yield(1);
        }

        public static int BuiltinDialog_List(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var character = lua.ToLuable<Character>(1);
            var message = lua.ToString(2);
            var icon = lua.ToString(3);
            var list = lua.ToStringList(4);

            character.Context.WriteAndFlushAsync(ShowListDialog.Bytes(message, icon, list));
            return lua.Yield(1);
        }
    }
}
