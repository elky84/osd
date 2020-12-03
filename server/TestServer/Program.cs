using KeraLua;
using NetworkShared;
using Serilog;
using ServerShared.DotNetty;
using ServerShared.Model;
using ServerShared.NetworkHandler;
using ServerShared.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using TestServer.Model;

namespace TestServer
{
    class GameHandler : BaseHandler<Character>
    {
        public List<Session<Character>> _movingSessions = new List<Session<Character>>();

        public GameHandler()
        { }

        public override void ChannelInactive(DotNetty.Transport.Channels.IChannelHandlerContext context)
        {
            base.ChannelInactive(context);
        }

        [FlatBufferEvent]
        public bool OnMove(Session<Character> session, Move x)
        {
            var character = session.Data;

            try
            {
                var clientDateTime = new DateTime(x.Now);
                if (clientDateTime > DateTime.Now)
                    return false;

                var latency = DateTime.Now - clientDateTime;
                if (latency.TotalSeconds > 10)
                    throw new Exception("...");

                if (character.Position != new Point(x.X, x.Y))
                    throw new Exception("position is not matched.");

                character.Time = new DateTime(x.Now);
                character.Direction = (Direction)x.Direction;
                Console.WriteLine($"Client is moving now. ({x.X}, {x.Y})");

                _movingSessions.Add(session);

                //session.WriteAndFlushAsync(...);
                //foreach (var s in this)
                //    s.WriteAndFlushAsync(...);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        [FlatBufferEvent]
        public bool OnStop(Session<Character> session, Stop x)
        {
            var character = session.Data;

            try
            {
                var clientDateTime = new DateTime(x.Now);
                if (clientDateTime > DateTime.Now)
                    return false;

                var latency = DateTime.Now - clientDateTime;
                if (latency.TotalSeconds > 10)
                    throw new Exception("...");

                character.UpdatePosition(new DateTime(x.Now));
                character.Time = null;
                Console.WriteLine($"Stop position : {character.Position}");

                _movingSessions.Remove(session);

                if (character.Position != new Point(x.X, x.Y))
                    throw new Exception("invalid");
                   
                Console.WriteLine("valid");
                //session.WriteAndFlushAsync(...);
                //foreach (var s in this)
                //    s.WriteAndFlushAsync(...);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        protected override void OnConnected(Session<Character> session)
        { }

        protected override void OnDisconnected(Session<Character> session)
        {
            _movingSessions.Remove(session);
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
            var map = new Map("map1", new Size(1024, 768));
            
            var character = new Character { Damage = 100, Position = new Point(1023, 767) };
            var sector = map.Add(character);
            Console.WriteLine(sector.Id);

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

                var bootstrap = bootstrapHelper.Create<GameHandler, Character>();
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
