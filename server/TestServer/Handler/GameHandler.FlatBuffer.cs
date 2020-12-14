using FlatBuffers.Protocol;
using KeraLua;
using NetworkShared;
using ServerShared.Model;
using ServerShared.NetworkHandler;
using System;
using System.IO;
using System.Linq;
using System.Text;
using TestServer.Extension;
using TestServer.Model;

namespace TestServer.Handler
{
    public partial class GameHandler
    {
        [FlatBufferEvent]
        public bool OnMove(Session<Character> session, Move request)
        {
            var character = session.Data;

            try
            {
                if (request.Now.Validate() == false)
                    throw new Exception("...");

                character.Synchronize(new DateTime(request.Now));
                if (character.Position.Delta(request.Position.Value) > 1)
                    throw new Exception("position is not matched.");

                character.Time = new DateTime(request.Now);
                character.Direction = (Direction)request.Direction;
                Console.WriteLine($"Client is moving now. ({request.Position?.X}, {request.Position?.Y})");

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
        public bool OnStop(Session<Character> session, Stop request)
        {
            var character = session.Data;

            try
            {
                if (request.Now.Validate() == false)
                    throw new Exception("...");

                character.Synchronize(new DateTime(request.Now));
                character.Time = null;
                Console.WriteLine($"Stop position : {character.Position}");

                _movingSessions.Remove(session);

                if (character.Position.Delta(request.Position.Value) > 1)
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

        [FlatBufferEvent]
        public bool OnClick(Session<Character> session, Click request)
        {
            var character = session.Data;
            var target = character.Map.Objects[request.Sequence];
            if (target == null)
                return true;

            switch (target.Type)
            {
                case ObjectType.NPC:
                    {
                        var npc = target as NPC;
                        if (File.Exists(npc.Script) == false)
                            break;

                        var luaThread = Static.Main.NewThread();
                        luaThread.Encoding = Encoding.UTF8;
                        luaThread.DoFile(npc.Script);
                        luaThread.GetGlobal("func");

                        luaThread.PushLuable(character);
                        luaThread.PushLuable(npc);
                        luaThread.Resume(2);

                        character.DialogThread = luaThread;
                    }
                    break;
            }
            return true;
        }

        [FlatBufferEvent]
        public bool OnDialog(Session<Character> session, ResponseDialog request)
        {
            var character = session.Data;
            if (character.DialogThread == null)
                return false;

            character.DialogThread.PushBoolean(request.Next);
            var result = character.DialogThread.Resume(1);
            return true;
        }

        [FlatBufferEvent]
        public bool OnSelectListDialog(Session<Character> session, SelectListDialog request)
        {
            var character = session.Data;
            if (character.DialogThread == null)
                return false;

            character.DialogThread.PushInteger(request.Index);
            character.DialogThread.Resume(1);
            return true;
        }

        [FlatBufferEvent]
        public bool OnCheatPosition(Session<Character> session, CheatPosition request)
        {
            var character = session.Data;
            character.Position = new NetworkShared.Types.Point { X = request.Position.Value.X, Y = request.Position.Value.Y };

            _ = Broadcast(session.Data, PositionChanged.Bytes(character.Sequence.Value, new FlatBuffers.Protocol.Position.Model { X = character.Position.X, Y = character.Position.Y }));
            return true;
        }

        [FlatBufferEvent]
        public bool OnWarp(Session<Character> session, Warp request)
        {
            var character = session.Data;
            character.Synchronize(new DateTime(request.Now));
            if (character.Position.Delta(request.Position.Value) > 1)
                throw new Exception("invalid");

            var portals = character.Map.Portals;

            var portal = portals.FirstOrDefault(x => x.BeforePosition.Delta(request.Position.Value) < 5.0f);
            if (portal != null)
            {
                var after = _maps[portal.AfterMap];
                after.Add(character);
            }

            return true;
        }
    }
}
