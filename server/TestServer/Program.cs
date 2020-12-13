﻿using FlatBuffers.Protocol;
using KeraLua;
using NetworkShared;
using NetworkShared.Common;
using Newtonsoft.Json;
using Serilog;
using ServerShared.DotNetty;
using ServerShared.Model;
using ServerShared.NetworkHandler;
using ServerShared.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TestServer.Extension;
using TestServer.Model;

namespace TestServer
{
    public class GameHandler : BaseHandler<Character>
    {
        public List<Session<Character>> _movingSessions = new List<Session<Character>>();

        public GameHandler()
        { }

        private void Synchronize(DateTime now)
        {
            _movingSessions.ForEach(x => x.Data?.Synchronize(now));
        }

        public async Task Broadcast(Session<Character> pivot, byte[] bytes, bool exceptSelf = true)
        {
            var character = pivot.Data;
            var targets = character.Map.Sectors.Nears(character.Position).SelectMany(x => x.Characters);

            if (exceptSelf)
                targets = targets.Except(new[] { character });

            foreach (var target in targets)
            {
                await target.Context.Send(bytes);
            }
        }

        public async Task Broadcast(Map map, byte[] bytes)
        {
            foreach (var context in map.Sectors.SelectMany(x => x.Characters).Select(x => x.Context))
            {
                await context.Send(bytes);
            }
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

            session.Send(ShowList.Bytes(objects));
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

                var mapData = JsonConvert.DeserializeObject<MapData>(File.ReadAllText("Resources/Map/town.json"));

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
