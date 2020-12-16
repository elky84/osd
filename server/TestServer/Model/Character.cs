using DotNetty.Transport.Channels;
using FlatBuffers.Protocol;
using KeraLua;
using NetworkShared;
using ServerShared.NetworkHandler;
using System;

namespace TestServer.Model
{
    public class Character : Life
    {
        public new interface IListener : Life.IListener
        { }

        public new IListener Listener { get; private set; }

        public IChannelHandlerContext Context { get; set; }

        public Lua DialogThread { get; set; }

        public int Damage { get; set; } = 30;

        public override ObjectType Type => ObjectType.Character;

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

        public static int BuiltinDialog(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var argc = lua.GetTop();
            var character = lua.ToLuable<Character>(1);
            var npc = lua.ToLuable<NPC>(2);
            var message = lua.ToString(3);
            var next = argc >= 4 ? lua.ToBoolean(4) : true;
            var quit = argc >= 5 ? lua.ToBoolean(5) : true;

            _ = character.Context.Send(ShowDialog.Bytes(message, next, quit));
            return lua.Yield(1);
        }

        public static int BuiltinDialog_List(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var character = lua.ToLuable<Character>(1);
            var npc = lua.ToLuable<NPC>(2);
            var message = lua.ToString(3);
            var list = lua.ToStringList(4);

            _ = character.Context.Send(ShowListDialog.Bytes(message, list));
            return lua.Yield(1);
        }

        public void BindEvent(IListener listener)
        {
            base.BindEvent(listener);
            Listener = listener;
        }
    }
}
