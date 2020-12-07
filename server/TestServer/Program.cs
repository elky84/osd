using FlatBuffers.Protocol;
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
using System.Linq;
using TestServer.Extension;
using DotNetty.Buffers;
using System.Text;

namespace TestServer
{
    public class GameHandler : BaseHandler<Character>
    {
        public List<Session<Character>> _movingSessions = new List<Session<Character>>();

        public GameHandler()
        { }

        public override void ChannelInactive(DotNetty.Transport.Channels.IChannelHandlerContext context)
        {
            base.ChannelInactive(context);
        }

        private void Synchronize(DateTime now)
        {
            _movingSessions.ForEach(x => x.Data?.Synchronize(now));
        }

        [FlatBufferEvent]
        public bool OnMove(Session<Character> session, Move x)
        {
            var character = session.Data;

            try
            {
                if (x.Now.Validate() == false)
                    throw new Exception("...");

                character.Synchronize(new DateTime(x.Now));
                if (character.Position.Delta(x.Position.Value) > 1)
                    throw new Exception("position is not matched.");

                character.Time = new DateTime(x.Now);
                character.Direction = (Direction)x.Direction;
                Console.WriteLine($"Client is moving now. ({x.Position?.X}, {x.Position?.Y})");

                _movingSessions.Add(session);

                //session.WriteAndFlushAsync(...);
                //foreach (var s in this)
                //    s.WriteAndFlushAsync(...);

                {
                    // 임시 테스트 코드
                    var msg = Unpooled.Buffer();
                    var begin = DateTime.Now;
                    var bytes = Move.Bytes(new FlatBuffers.Protocol.Position.Model(1, 2), begin.Ticks, 2);
                    msg = Unpooled.Buffer();
                    msg.WriteInt(bytes.Length);
                    var name = nameof(Move);
                    msg.WriteByte(name.Length);
                    msg.WriteString(name, Encoding.Default);
                    msg.WriteBytes(bytes);
                    _ = session.WriteAndFlushAsync(msg);
                }

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
                if (x.Now.Validate() == false)
                    throw new Exception("...");

                character.Synchronize(new DateTime(x.Now));
                character.Time = null;
                Console.WriteLine($"Stop position : {character.Position}");

                _movingSessions.Remove(session);

                if (character.Position.Delta(x.Position.Value) > 1)
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

        [FlatBufferEvent]
        public bool OnClick(Session<Character> session, Click x)
        {
            var character = session.Data;
            character.DialogThread = Static.Main.NewThread();
            character.DialogThread.DoFile(Path.Join(Environment.CurrentDirectory, "..", "..", "..", "hello.lua"));
            character.DialogThread.GetGlobal("func");

            character.DialogThread.PushLuable(character);
            character.DialogThread.Resume(1);
            return true;
        }

        [FlatBufferEvent]
        public bool OnSelectListDialog(Session<Character> session, SelectListDialog x)
        {
            var character = session.Data;
            if (character.DialogThread == null)
                return false;

            character.DialogThread.PushInteger(x.Index);
            character.DialogThread.Resume(1);
            return true;
        }

        protected override void OnConnected(Session<Character> session)
        {
            session.Data.Context = session;
            var map = new Map("map name", new Size(1024, 768));
            map.Sectors.OnSectorChanged = obj =>
            {
                Console.WriteLine($"Sector changed({obj.Sequence}, sector : {obj.Sector.Id})");
            };
            map.Add(session.Data);

            // 현재 맵에 있는 모든 오브젝트
            var objects = map.Objects
                .Select(x => new FlatBuffers.Protocol.Object.Model(x.Key, new FlatBuffers.Protocol.Position.Model(x.Value.Position.X, x.Value.Position.Y)))
                .ToList();

            ShowList.Bytes(objects);
        }

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
