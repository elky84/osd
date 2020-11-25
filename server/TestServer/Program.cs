using FlatBuffers;
using KeraLua;
using NetworkShared;
using NetworkShared.Table;
using Serilog;
using ServerShared.DotNetty;
using ServerShared.Model;
using ServerShared.NetworkHandler;
using ServerShared.Util;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TestServer
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

    public class Life : Luable
    {
        public int Hp { get; set; } = 50;

        public static int BuiltinHP(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var life = lua.ToLuable<Life>(1);

            lua.PushInteger(life.Hp);
            return 1;
        }
    }

    public class Character : Life
    {
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
    }




    class Session : BaseSession
    { }

    class GameHandler : BaseHandler<Session>
    {
        public GameHandler()
        {
        }

        [FlatBufferEvent]
        public bool OnPlayerInfo(Session session, PlayerInfo x)
        {
            return true;
        }
    }

    class Program
    {
        static async Task Main()
        {
            MasterTable.Load("ServerShared");
            var value1 = MasterTable.From<TableSheet1>()[0];
            var value2 = MasterTable.From<TableSheet23>()["아이디1"];
            var value3 = MasterTable.From<TableSheet1>().Cached["이름1"];

            var handler = new GameHandler();

            var builder = new FlatBufferBuilder(512);
            var name = builder.CreateString("cshyeon");
            var offset = PlayerInfo.CreatePlayerInfo(builder, name, 123);
            PlayerInfo.FinishPlayerInfoBuffer(builder, offset);
            handler.Call<PlayerInfo>(null, builder.DataBuffer.ToSizedArray());


            var lua = Static.Main.NewThread();
            lua.DoFile(Path.Join(Environment.CurrentDirectory, "..", "..", "..", "hello.lua"));
            lua.GetGlobal("func");
            var character = new Character { Damage = 100 };
            lua.PushLuable(character);
            var result = lua.Resume(1);
            lua.PushInteger(5);
            lua.Resume(1);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            var bootstrapHelper = new BootstrapHelper();
            try
            {
                ServerService.Register();

                var bootstrap = bootstrapHelper.Create<GameHandler, Session>();
                var channel = await bootstrap.BindAsync(ServerShared.Config.ServerSettings.Port);
                Log.Logger.Information("Server Started");
                Console.ReadLine();
                await channel.CloseAsync();
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                Log.Logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                await bootstrapHelper.GracefulClose();
            }
        }
    }
}
