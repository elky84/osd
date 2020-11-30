using KeraLua;
using NetworkShared;
using Serilog;
using ServerShared.DotNetty;
using ServerShared.Model;
using ServerShared.NetworkHandler;
using ServerShared.Util;
using System;
using System.Drawing;
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



    public enum Direction
    { 
        Left, Top, Right, Bottom
    }

    class Session : BaseSession
    {
        public Direction Direction { get; set; }
        public DateTime? Time { get; set; }
        public uint Speed { get; set; } = 10;
        public Point Position { get; set; } = new Point();

        public Point UpdatePosition(DateTime time)
        {
            if (Time == null)
                return Position;

            var diff = time - Time.Value;
            var moved = (int)(diff.TotalMilliseconds * (Speed / 1000.0));

            switch (Direction)
            {
                case Direction.Left:
                    Position = new Point(Position.X - moved, Position.Y);
                    break;

                case Direction.Top:
                    Position = new Point(Position.X, Position.Y - moved);
                    break;

                case Direction.Right:
                    Position = new Point(Position.X + moved, Position.Y);
                    break;

                case Direction.Bottom:
                    Position = new Point(Position.X, Position.Y + moved);
                    break;

                default:
                    throw new Exception("Invalid direction value");
            }

            return Position;
        }
    }

    class GameHandler : BaseHandler<Session>
    {
        public GameHandler()
        {
        }

        [FlatBufferEvent]
        public bool OnMove(Session session, Move x)
        {
            try
            {
                var latency = DateTime.Now - new DateTime(x.Now);
                if (latency.TotalSeconds > 10)
                    throw new Exception("...");

                if (session.Position != new Point(x.X, x.Y))
                    throw new Exception("position is not matched.");

                session.Time = new DateTime(x.Now);
                session.Direction = (Direction)x.Direction;
                Console.WriteLine($"Client is moving now. ({x.X}, {x.Y})");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        [FlatBufferEvent]
        public bool OnStop(Session session, Stop x)
        {
            try
            {
                var latency = DateTime.Now - new DateTime(x.Now);
                if (latency.TotalSeconds > 10)
                    throw new Exception("...");

                session.UpdatePosition(new DateTime(x.Now));
                session.Time = null;
                Console.WriteLine($"Stop position : {session.Position}");

                if (session.Position != new Point(x.X, x.Y))
                    throw new Exception("invalid");
                   
                Console.WriteLine("valid");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }

    class Program
    {
        static async Task Main()
        {
            MasterTable.Load("ServerShared");

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
