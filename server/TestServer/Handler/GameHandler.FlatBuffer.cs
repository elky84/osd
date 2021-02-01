using KeraLua;
using NetworkShared;
using ServerShared.Model;
using ServerShared.NetworkHandler;
using System;
using System.IO;
using System.Linq;
using System.Text;
using TestServer.Model;

namespace TestServer.Handler
{
    public partial class GameHandler
    {
        [FlatBufferEvent]
        public bool OnMove(Session<Character> session, FlatBuffers.Protocol.Request.Move request)
        {
            var character = session.Data;

            try
            {
                if (character.Position.Delta(request.Position.Value) > 1.0)
                    throw new Exception("위치가 올바르지 않습니다.");

                character.Position = new NetworkShared.Types.Point(request.Position.Value.X, request.Position.Value.Y);
                character.Direction = (Direction)request.Direction;
                character.Velocity = new NetworkShared.Types.Point(1.0 * (request.Direction == (int)Direction.Left ? -1 : 1), character.Velocity.Y);
                _ = Broadcast(character, FlatBuffers.Protocol.Response.State.Bytes(character));

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

            try
            {
                if (character.ValidPosition(new NetworkShared.Types.Point(request.Position.Value.X, request.Position.Value.Y)) == false)
                    throw new Exception("올바른 위치가 아님");

                character.Position = new NetworkShared.Types.Point(request.Position.Value.X, request.Position.Value.Y);
                character.Velocity = new NetworkShared.Types.Point(0, character.Velocity.Y);
                _ = Broadcast(character, FlatBuffers.Protocol.Response.State.Bytes(character));
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

            try
            {
                if (character.ValidPosition(new NetworkShared.Types.Point(request.Position.Value.X, request.Position.Value.Y)) == false)
                    throw new Exception("올바른 위치가 아님");

                if (character.Jumping)
                    throw new Exception("점프 혹은 낙하중에 점프하려고 함");

                character.Position = new NetworkShared.Types.Point(request.Position.Value.X, request.Position.Value.Y);
                character.Velocity = new NetworkShared.Types.Point(character.Velocity.X, -10.0);
                _ = Broadcast(character, FlatBuffers.Protocol.Response.State.Bytes(character));
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

            try
            {
                if (character.ValidPosition(new NetworkShared.Types.Point(request.Position.Value.X, request.Position.Value.Y)) == false)
                    throw new Exception("올바른 위치가 아님");

                if (character.Jumping)
                    throw new Exception("점프 혹은 낙하중에 낙하될 순 없음");

                character.Position = new NetworkShared.Types.Point(request.Position.Value.X, request.Position.Value.Y);
                character.Velocity = new NetworkShared.Types.Point(character.Velocity.X, 0.0);
                _ = Broadcast(character, FlatBuffers.Protocol.Response.State.Bytes(character));

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

            try
            {
                switch ((Axis)request.Axis)
                {
                    case Axis.X:
                        {
                            if (character.ValidPosition(new NetworkShared.Types.Point(request.Position.Value.X, request.Position.Value.Y)) == false)
                                throw new Exception("올바른 위치가 아님");

                            character.Position = new NetworkShared.Types.Point(request.Position.Value.X, request.Position.Value.Y);
                            character.Velocity = new NetworkShared.Types.Point(0, character.Velocity.Y);
                            _ = Broadcast(character, FlatBuffers.Protocol.Response.State.Bytes(character));
                        }
                        break;

                    case Axis.Y:
                        {
                            if (character.Jumping == false)
                                throw new Exception("점프 혹은 낙하상태가 아닌 경우 Y축 충돌은 안일어남");

                            if (character.ValidPosition(new NetworkShared.Types.Point(request.Position.Value.X, request.Position.Value.Y)) == false)
                                throw new Exception("올바른 위치가 아님");

                            character.Position = new NetworkShared.Types.Point(request.Position.Value.X, request.Position.Value.Y);
                            character.Velocity = new NetworkShared.Types.Point(character.Velocity.X, 0.0);
                            character.JumpLimit = character.Position.Y;
                            _ = Broadcast(character, FlatBuffers.Protocol.Response.State.Bytes(character));
                        }
                        break;

                    default:
                        throw new Exception("잘못된 요청");
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
            character.Position = new NetworkShared.Types.Point { X = request.Position.Value.X, Y = request.Position.Value.Y };

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
