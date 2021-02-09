using KeraLua;
using NetworkShared;
using ServerShared.Model;
using ServerShared.NetworkHandler;
using System;
using System.IO;
using System.Linq;
using System.Text;
using TestServer.Model;
using NetworkShared.Types;

namespace TestServer.Handler
{
    public partial class GameHandler
    {
        [FlatBufferEvent]
        public bool OnMove(Session<Character> session, FlatBuffers.Protocol.Request.Move request)
        {
            var character = session.Data;
            var position = request.Position.ToPoint();

            try
            {
                if (character.Position.Delta(request.Position.Value) > 1.0)
                    throw new Exception("위치가 올바르지 않습니다.");

                character.Position = position;
                character.Move((Direction)request.Direction);
                _ = Broadcast(character, FlatBuffers.Protocol.Response.State.Bytes(character.State(false)));

                Console.WriteLine($"캐릭터가 이동 ({request.Position?.X}, {request.Position?.Y})");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        [FlatBufferEvent]
        public bool OnStop(Session<Character> session, FlatBuffers.Protocol.Request.Stop request)
        {
            var character = session.Data;
            var position = request.Position.ToPoint();

            try
            {
                if (character.ValidPosition(position) == false)
                    throw new Exception("올바른 위치가 아님");

                character.Position = position;
                character.Stop();
                _ = Broadcast(character, FlatBuffers.Protocol.Response.State.Bytes(character.State(false)));
                Console.WriteLine($"캐릭터가 멈춤 : {character.Position}");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        [FlatBufferEvent]
        public bool OnJump(Session<Character> session, FlatBuffers.Protocol.Request.Jump request)
        {
            var character = session.Data;
            var position = request.Position.ToPoint();

            try
            {
                if (character.ValidPosition(position) == false)
                    throw new Exception("올바른 위치가 아님");

                character.Position = position;
                character.Jump(true);
                _ = Broadcast(character, FlatBuffers.Protocol.Response.State.Bytes(character.State(true)));
                Console.WriteLine($"캐릭터가 점프함 : {character.Position}");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        [FlatBufferEvent]
        public bool OnFall(Session<Character> session, FlatBuffers.Protocol.Request.Fall request)
        {
            var character = session.Data;
            var position = request.Position.ToPoint();

            try
            {
                if (character.ValidPosition(position) == false)
                    throw new Exception("올바른 위치가 아님");

                character.Position = position;
                character.Fall();
                _ = Broadcast(character, FlatBuffers.Protocol.Response.State.Bytes(character.State(false)));

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        [FlatBufferEvent]
        public bool OnCollision(Session<Character> session, FlatBuffers.Protocol.Request.Collision request)
        {
            var character = session.Data;
            var position = request.Position.ToPoint();

            try
            {
                switch ((Axis)request.Axis)
                {
                    case Axis.X:
                        {
                            if (character.ValidPosition(position) == false)
                                throw new Exception("올바른 위치가 아님");

                            character.Position = position;
                        }
                        break;

                    case Axis.Y:
                        {
                            if (character.ValidPosition(position) == false)
                                throw new Exception("올바른 위치가 아님");

                            character.Position = position;
                            character.Jump(false);
                        }
                        break;

                    default:
                        throw new Exception("잘못된 요청");
                }

                _ = Broadcast(character, FlatBuffers.Protocol.Response.State.Bytes(character.State(false)));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        [FlatBufferEvent]
        public bool OnUpdate(Session<Character> session, FlatBuffers.Protocol.Request.Update request)
        {
            var character = session.Data;
            var position = request.Position.ToPoint();
            if (character.ValidPosition(position) == false)
                throw new Exception("올바른 위치가 아님");

            character.Position = position;
            Console.WriteLine($"{character.Sequence.Value} : {position.X}/{position.Y}");

            for (int i = 0; i < request.MobsLength; i++)
            {
                var mobSequence = request.Mobs(i).Value.Sequence;
                var mobPosition = request.Mobs(i).Value.Position.ToPoint();
                var mob = character.Map.Objects[mobSequence] ??
                    throw new Exception("몹 없음");

                if (mob.ValidPosition(mobPosition) == false)
                    throw new Exception("위치 올바르지 않음");

                mob.Position = mobPosition;
            }

            return true;
        }

        [FlatBufferEvent]
        public bool OnClick(Session<Character> session, FlatBuffers.Protocol.Request.Click request)
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
        public bool OnDialog(Session<Character> session, FlatBuffers.Protocol.Request.DialogResult request)
        {
            var character = session.Data;
            if (character.LuaThread == null)
                return false;

            character.LuaThread.PushBoolean(request.Next);
            var result = character.LuaThread.Resume(1);
            return true;
        }

        [FlatBufferEvent]
        public bool OnSelectListDialog(Session<Character> session, FlatBuffers.Protocol.Request.DialogIndexResult request)
        {
            var character = session.Data;
            if (character.LuaThread == null)
                return false;

            character.LuaThread.PushInteger(request.Index);
            character.LuaThread.Resume(1);
            return true;
        }

        [FlatBufferEvent]
        public bool OnCheatPosition(Session<Character> session, FlatBuffers.Protocol.Request.CheatPosition request)
        {
            var character = session.Data;
            character.Position = new Point { X = request.Position.Value.X, Y = request.Position.Value.Y };

            _ = Broadcast(session.Data, FlatBuffers.Protocol.Response.PositionChanged.Bytes(character.Sequence.Value, character.Position));
            return true;
        }

        [FlatBufferEvent]
        public bool OnWarp(Session<Character> session, FlatBuffers.Protocol.Request.Warp request)
        {
            var character = session.Data;
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
        public bool OnCheatKill(Session<Character> session, FlatBuffers.Protocol.Request.CheatKill request)
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
        public bool OnActiveItem(Session<Character> session, FlatBuffers.Protocol.Request.ActiveItem request)
        {
            var character = session.Data;
            var activatedItem = character.Items.Active(request.Id);
            if (activatedItem != null)
            {
                Console.WriteLine($"activated item : {activatedItem.Id}({activatedItem.Name})");

                // 장비 사용하면 외형변경 브로드캐스팅
                if (activatedItem.Master.Type == ItemType.Equipment)
                    _ = Broadcast(character, FlatBuffers.Protocol.Response.ShowCharacter.Bytes(character), false);
            }

            return true;
        }

        [FlatBufferEvent]
        public bool OnInactiveItem(Session<Character> session, FlatBuffers.Protocol.Request.InactiveItem request)
        {
            var character = session.Data;
            var inactivatedItem = character.Items.Inactive(request.Id);
            if (inactivatedItem != null)
            {
                Console.WriteLine($"inactivated item : {inactivatedItem.Id}({inactivatedItem.Name})");

                // 장비 해제하면 외형변경 브로드캐스팅
                _ = Broadcast(character, FlatBuffers.Protocol.Response.ShowCharacter.Bytes(character), false);
            }

            return true;
        }
    }
}
