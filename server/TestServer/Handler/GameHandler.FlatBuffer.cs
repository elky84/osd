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
using MasterData;
using MasterData.Table;
using Serilog;

namespace TestServer.Handler
{
    public partial class GameHandler
    {
        [FlatBufferEvent]
        public bool OnMove(Session<Character> session, FlatBuffers.Protocol.Request.Move request)
        {
            try
            {
                var obj = GetControllableObject(session, request.Sequence);
                if (obj == null)
                    return true;
                // throw new Exception("컨트롤 할 수 없는 오브젝트");

                var position = request.Position.ToPoint();

                if (obj.Position.Delta(request.Position.Value) > 1.0)
                    throw new Exception("위치가 올바르지 않습니다.");

                obj.Position = position;
                obj.Move((Direction)request.Direction);
                _ = Broadcast(obj, FlatBuffers.Protocol.Response.State.Bytes(obj.State(false)));

                Log.Logger.Information($"캐릭터가 이동 ({request.Position?.X}, {request.Position?.Y})");
                return true;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
                return false;
            }
        }

        [FlatBufferEvent]
        public bool OnStop(Session<Character> session, FlatBuffers.Protocol.Request.Stop request)
        {
            try
            {
                var obj = GetControllableObject(session, request.Sequence);
                if (obj == null)
                    return true;
                // throw new Exception("컨트롤 할 수 없는 오브젝트");

                var position = request.Position.ToPoint();

                if (obj.ValidPosition(position) == false)
                    throw new Exception("올바른 위치가 아님");

                obj.Position = position;
                obj.Stop();
                _ = Broadcast(obj, FlatBuffers.Protocol.Response.State.Bytes(obj.State(false)));
                Log.Logger.Information($"캐릭터가 멈춤 : {obj.Position}");
                return true;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
                return false;
            }
        }

        [FlatBufferEvent]
        public bool OnJump(Session<Character> session, FlatBuffers.Protocol.Request.Jump request)
        {
            try
            {
                var obj = GetControllableObject(session, request.Sequence);
                if (obj == null)
                    return true;
                // throw new Exception("컨트롤 할 수 없는 오브젝트");

                var position = request.Position.ToPoint();

                if (obj.ValidPosition(position) == false)
                    throw new Exception("올바른 위치가 아님");

                obj.Position = position;
                obj.Jump(true);
                _ = Broadcast(obj, FlatBuffers.Protocol.Response.State.Bytes(obj.State(true)));
                Log.Logger.Information($"캐릭터가 점프함 : {obj.Position}");
                return true;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
                return false;
            }
        }

        [FlatBufferEvent]
        public bool OnFall(Session<Character> session, FlatBuffers.Protocol.Request.Fall request)
        {
            try
            {
                var obj = GetControllableObject(session, request.Sequence);
                if (obj == null)
                    return true;
                // throw new Exception("컨트롤 할 수 없는 오브젝트");

                var position = request.Position.ToPoint();

                if (obj.ValidPosition(position) == false)
                    throw new Exception("올바른 위치가 아님");

                obj.Position = position;
                obj.Fall();
                _ = Broadcast(obj, FlatBuffers.Protocol.Response.State.Bytes(obj.State(false)));

                return true;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
                return false;
            }
        }

        [FlatBufferEvent]
        public bool OnCollision(Session<Character> session, FlatBuffers.Protocol.Request.Collision request)
        {
            try
            {
                var obj = GetControllableObject(session, request.Sequence);
                if (obj == null)
                    return true;
                // throw new Exception("컨트롤 할 수 없는 오브젝트");

                var position = request.Position.ToPoint();

                switch ((Axis)request.Axis)
                {
                    case Axis.X:
                        {
                            if (obj.ValidPosition(position) == false)
                                throw new Exception("올바른 위치가 아님");

                            obj.Position = position;
                        }
                        break;

                    case Axis.Y:
                        {
                            if (obj.ValidPosition(position) == false)
                                throw new Exception("올바른 위치가 아님");

                            obj.Position = position;
                            obj.Jump(false);
                        }
                        break;

                    default:
                        throw new Exception("잘못된 요청");
                }

                _ = Broadcast(obj, FlatBuffers.Protocol.Response.State.Bytes(obj.State(false)));
                return true;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
                return false;
            }
        }

        [FlatBufferEvent]
        public bool OnUpdate(Session<Character> session, FlatBuffers.Protocol.Request.Update request)
        {
            var obj = GetControllableObject(session, request.Sequence);
            if (obj == null)
            {
                Log.Logger.Error($"컨트롤 할 수 없는 오브젝트.");
                return true;
            }

            var position = request.Position.ToPoint();
            if (obj.ValidPosition(position) == false)
            {
                Log.Logger.Error($"유효하지 않은 위치입니다.");
                return true;
            }

            obj.Position = position;
            //Log.Logger.Error($"{obj.Sequence.Value} : {position.X}/{position.Y}");

            for (int i = 0; i < request.MobsLength; i++)
            {
                var mobSequence = request.Mobs(i).Value.Sequence;
                var mobPosition = request.Mobs(i).Value.Position.ToPoint();
                var mob = obj.Map.Objects[mobSequence] ??
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
                Log.Logger.Error($"activated item : {activatedItem.Id}({activatedItem.Name})");

                // 장비 사용하면 외형변경 브로드캐스팅
                //if (activatedItem.Master.Type == ItemType.Equipment)
                //    _ = Broadcast(character, FlatBuffers.Protocol.Response.ShowCharacter.Bytes(character), false);
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
                Log.Logger.Error($"inactivated item : {inactivatedItem.Id}({inactivatedItem.Name})");

                // 장비 해제하면 외형변경 브로드캐스팅
                //_ = Broadcast(character, FlatBuffers.Protocol.Response.ShowCharacter.Bytes(character), false);
            }

            return true;
        }

        [FlatBufferEvent]
        public bool OnActiveSkill(Session<Character> session, FlatBuffers.Protocol.Request.ActiveSkill request)
        {
            using var lua = Static.Main.NewThread();
            lua.Encoding = Encoding.UTF8;

            var skillName = string.Empty;
            switch (request.Id)
            {
                case 0:
                    skillName = "전체공격스킬";
                    break;

                case 1:
                    skillName = "전체회복스킬";
                    break;
            }

            var path = MasterTable.From<TableSkill>()[skillName].Script;
            if (File.Exists(path) == false)
                return true;

            lua.DoFile(path);
            lua.GetGlobal("func");

            lua.PushLuable(session.Data);
            var state = lua.Resume(1);
            return true;
        }
    }
}
