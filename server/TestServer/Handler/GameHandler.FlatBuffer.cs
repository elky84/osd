using KeraLua;
using MasterData;
using MasterData.Server;
using NetworkShared;
using NetworkShared.Types;
using Serilog;
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
            var portals = character.Map.Portals;

            var portal = portals.FirstOrDefault(x => x.BeforePosition.Delta(character.Position) < 1.0f);
            if (portal != null)
                character.Map = _maps[portal.AfterMap];

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
                Log.Logger.Error($"activated item : {activatedItem.Id}({activatedItem.Name})");

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
            try
            {
                session.Data.Skills[2].Execute();
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
            }

            return true;
        }

        [FlatBufferEvent]
        public bool OnAttack(Session<Character> session, FlatBuffers.Protocol.Request.Attack request)
        {
            try
            {
                var character = session.Data;
                var weapon = character.Items.Weapon;
                if (weapon == null)
                    throw new Exception("장착한 무기가 없음.");

                var option = MasterTable.From<TableWeaponOption>()[weapon.Master.Id] ??
                    throw new Exception("무기 옵션 정보 없음");
                var weaponRange = MasterTable.From<TableWeaponRange>()[option.Type] ??
                    throw new Exception("무기 범위 정보 없음");

                _ = Broadcast(character, FlatBuffers.Protocol.Response.Attack.Bytes(character.Sequence.Value), exceptSelf: false, sector: character.Sector);

                var rangeBox = new RectF
                {
                    X = character.Position.X,
                    Y = character.Position.Y - character.CollisionSize.Height / 2.0,
                    Width = weaponRange.Width,
                    Height = weaponRange.Height
                };

                if (character.Direction == Direction.Left)
                    rangeBox.X -= (int)rangeBox.Width;

                var mob = character.Sector.Nears.SelectMany(x => x.Objects).Where(x => x.Type == ObjectType.Mob).Select(x => x as Model.Mob).FirstOrDefault(x => rangeBox.Contains(x.CollisionBox));
                if (mob != null)
                    mob.Damage(mob.Stats.Max[StatType.HP], character);

                if (mob != null)
                    Log.Logger.Information($"{mob.Sequence.Value}가 맞아죽음");
                else
                    Log.Logger.Information($"범위 안에 몹 없음");
           }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
            }

            return true;
        }

        [FlatBufferEvent]
        public bool OnPickup(Session<Character> session, FlatBuffers.Protocol.Request.Pickup request)
        {
            var character = session.Data;
            var items = character.Sector.Objects.Where(x => x.Type == ObjectType.Item).Where(x => x.Position.Delta(character.Position) < 1.0).Select(x => x as Model.Item);

            foreach (var item in items)
            {
                item.Map.Remove(item);
                character.Items.Inventory.Add(item);
            }

            return true;
        }
    }
}
