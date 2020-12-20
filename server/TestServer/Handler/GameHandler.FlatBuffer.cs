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

                character.MoveTime = new DateTime(request.Now);
                character.Direction = (Direction)request.Direction;
                Console.WriteLine($"Client is moving now. ({request.Position?.X}, {request.Position?.Y})");

                _ = Broadcast(character, ShowCharacter.Bytes(character.ShowCharacterFlatBuffer));
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
                character.MoveTime = null;
                Console.WriteLine($"Stop position : {character.Position}");

                if (character.Position.Delta(request.Position.Value) > 1)
                    throw new Exception("invalid");

                Console.WriteLine("valid");
                _ = Broadcast(character, ShowCharacter.Bytes(character.ShowCharacterFlatBuffer));
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
                        if (File.Exists(npc.Master.Script) == false)
                            break;

                        character.LuaThread = Static.Main.NewThread();
                        character.LuaThread.Encoding = Encoding.UTF8;
                        character.LuaThread.DoFile(npc.Master.Script);
                        character.LuaThread.GetGlobal("func");

                        character.LuaThread.PushLuable(character);
                        character.LuaThread.PushLuable(npc);
                        character.LuaThread.Resume(2);
                    }
                    break;
            }
            return true;
        }

        [FlatBufferEvent]
        public bool OnDialog(Session<Character> session, ResponseDialog request)
        {
            var character = session.Data;
            if (character.LuaThread == null)
                return false;

            character.LuaThread.PushBoolean(request.Next);
            var result = character.LuaThread.Resume(1);
            return true;
        }

        [FlatBufferEvent]
        public bool OnSelectListDialog(Session<Character> session, SelectListDialog request)
        {
            var character = session.Data;
            if (character.LuaThread == null)
                return false;

            character.LuaThread.PushInteger(request.Index);
            character.LuaThread.Resume(1);
            return true;
        }

        [FlatBufferEvent]
        public bool OnCheatPosition(Session<Character> session, CheatPosition request)
        {
            var character = session.Data;
            character.Position = new NetworkShared.Types.Point { X = request.Position.Value.X, Y = request.Position.Value.Y };

            _ = Broadcast(session.Data, PositionChanged.Bytes(character.Sequence.Value, character.Position.FlatBuffer));
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
                character.Map = after;
            }

            return true;
        }

        [FlatBufferEvent]
        public bool OnCheatKill(Session<Character> session, CheatKill request)
        {
            var character = session.Data;
            var target = character.Map.Objects[request.Sequence];
            if (target is Life)
            {
                var life = target as Life;
                life.Kill();
            }

            return true;
        }

        [FlatBufferEvent]
        public bool OnActiveItem(Session<Character> session, ActiveItem request)
        {
            var character = session.Data;
            var activatedItem = character.Items.Active(request.Id);
            if (activatedItem != null)
            {
                Console.WriteLine($"activated item : {activatedItem.Id}({activatedItem.Name})");

                // 장비 사용하면 외형변경 브로드캐스팅
                if (activatedItem.Master.Type == ItemType.Equipment)
                    _ = Broadcast(character, ShowCharacter.Bytes(character.ShowCharacterFlatBuffer), false);
            }

            return true;
        }

        [FlatBufferEvent]
        public bool OnInactiveItem(Session<Character> session, InactiveItem request)
        {
            var character = session.Data;
            var inactivatedItem = character.Items.Inactive(request.Id);
            if (inactivatedItem != null)
            {
                Console.WriteLine($"inactivated item : {inactivatedItem.Id}({inactivatedItem.Name})");

                // 장비 해제하면 외형변경 브로드캐스팅
                _ = Broadcast(character, ShowCharacter.Bytes(character.ShowCharacterFlatBuffer), false);
            }

            return true;
        }
    }
}
