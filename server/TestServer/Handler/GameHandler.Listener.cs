using NetworkShared;
using Serilog;
using ServerShared.NetworkHandler;
using System.Linq;
using TestServer.Model;

namespace TestServer.Handler
{
    public partial class GameHandler : Mob.IListener, Character.IListener
    {
        public void OnLeave(Model.Object obj)
        {
            
        }

        public void OnEnter(Model.Object obj)
        {
            // 현재 맵에 있는 모든 오브젝트
            var portals = obj.Map.Portals
                .ConvertAll(x => (FlatBuffers.Protocol.Response.Portal.Model)x)
                .ToList();

            // 입장한 유저에게 정보 전송
            if (obj is Character)
            {
                var character = obj as Character;
                _ = character.Send(FlatBuffers.Protocol.Response.Enter.Bytes(character.ToProtocol(),
                    character.Map,
                    character.Position,
                    (int)character.Direction,
                    portals));
            }
        }

        public void OnSectorChanged(Model.Object obj, Map.Sector sector1, Map.Sector sector2)
        {
            var befores = sector1 != null ?
                sector1.Nears.Where(x => x != null).SelectMany(x => x.Objects).ToList() :
                new System.Collections.Generic.List<Object>();
            var afters = sector2 != null ?
                sector2.Nears.Where(x => x != null).SelectMany(x => x.Objects).ToList() :
                new System.Collections.Generic.List<Object>();

            var hides = befores.Except(afters).ToList();
            _ = obj.Send(FlatBuffers.Protocol.Response.Leave.Bytes(hides.Select(x => x.Sequence.Value).ToList()));

            var hideBytes = FlatBuffers.Protocol.Response.Leave.Bytes(new System.Collections.Generic.List<int> { obj.Sequence.Value });
            foreach (var ch in hides.Where(x => x.Type == NetworkShared.ObjectType.Character).Select(x => x as Character))
            {
                _ = ch.Send(hideBytes);
            }

            var shows = afters.Except(befores).ToList();
            _ = obj.Send(FlatBuffers.Protocol.Response.Show.Bytes(
                shows.Where(x => x.Type != NetworkShared.ObjectType.Character).Select(x => x.ToProtocol()).ToList(),
                shows.Where(x => x.Type == NetworkShared.ObjectType.Character).Select(x => (x as Character).ToProtocol()).ToList()));


            var showBytes = obj is Character ?
                FlatBuffers.Protocol.Response.Show.Bytes(
                    new System.Collections.Generic.List<FlatBuffers.Protocol.Response.Object.Model> { },
                    new System.Collections.Generic.List<FlatBuffers.Protocol.Response.Character.Model> { obj as Character }) :
                FlatBuffers.Protocol.Response.Show.Bytes(
                    new System.Collections.Generic.List<FlatBuffers.Protocol.Response.Object.Model> { obj },
                    new System.Collections.Generic.List<FlatBuffers.Protocol.Response.Character.Model> { });
            foreach (var ch in shows.Where(x => x.Type == NetworkShared.ObjectType.Character).Select(x => x as Character))
            {
                _ = ch.Send(showBytes);
            }

            if (obj.Type == NetworkShared.ObjectType.Character)
            {
                var character = obj as Character;
                var unsetList = hides.Where(x => x.Type == NetworkShared.ObjectType.Mob).Select(x => x as Mob).Where(x => x.Owner == character).ToList();
                _ = character.Send(FlatBuffers.Protocol.Response.UnsetOwner.Bytes(unsetList.Select(x => x.Sequence.Value).ToList()));

                foreach (var (sector, mobs) in unsetList.GroupBy(x => x.Sector).ToDictionary(x => x.Key, x => x.ToList()))
                {
                    var newOwner = sector.Nears.SelectMany(x => x.Characters).FirstOrDefault();
                    foreach (var mob in mobs)
                        mob.Owner = newOwner;

                    if (newOwner != null)
                    {
                        var sequences = mobs.Select(x => x.Sequence.Value).ToList();
                        _ = newOwner.Context.Send(FlatBuffers.Protocol.Response.SetOwner.Bytes(sequences));
                    }
                }

                var setList = shows.Where(x => x.Type == NetworkShared.ObjectType.Mob).Select(x => x as Mob).Where(x => x.Owner == null).ToList();
                foreach (var mob in setList)
                {
                    mob.Owner = character;
                }
                _ = character.Send(FlatBuffers.Protocol.Response.SetOwner.Bytes(setList.Select(x => x.Sequence.Value).ToList()));
            }
            else if(obj.Type == NetworkShared.ObjectType.Mob)
            {
                var mob = obj as Mob;
                var ownerBefore = mob.Owner;

                mob.Owner = mob.Sector != null ?
                    mob.Sector.Nears.SelectMany(x => x.Characters).FirstOrDefault() :
                    null;

                if (ownerBefore != mob.Owner)
                {
                    _ = ownerBefore?.Send(FlatBuffers.Protocol.Response.UnsetOwner.Bytes(new System.Collections.Generic.List<int> { obj.Sequence.Value }));
                    _ = mob.Owner?.Send(FlatBuffers.Protocol.Response.SetOwner.Bytes(new System.Collections.Generic.List<int> { obj.Sequence.Value }));
                }
            }
        }

        public void OnSpawned(Mob mob)
        {
            // 이런 형식으로 쓰면 편하긴 한데 퍼포먼스 이슈가...
            var collision = MasterData.MasterTable.From<MasterData.Table.TableCollision>()[mob.Name];
            mob.CollisionSize = new NetworkShared.Types.SizeF { Width = collision.Width, Height = collision.Height };
            Log.Logger.Information($"Spawned '{mob.Name}({mob.Sequence.Value})' in '{mob.Map.Name}' ({mob.Position.X}, {mob.Position.Y})");
        }

        public void OnDie(Life life)
        {
            Log.Logger.Information($"{life.Name}({life.Sequence}) is dead.");
            _ = Broadcast(life, FlatBuffers.Protocol.Response.Die.Bytes(life.Sequence.Value));
        }

        public void OnSectorEntering(Model.Object obj, Map.Sector sector)
        {

        }

        public void OnSectorLeaving(Model.Object obj, Map.Sector sector)
        {
            
        }

        public void OnSectorEntered(Model.Object obj, Map.Sector sector)
        {
            
        }

        public void OnSectorLeaved(Model.Object obj, Map.Sector sector)
        {
            
        }

        public void OnDamaged(Life life, int damage)
        {
            Log.Logger.Information($"damaged : {life.Sequence.Value}({damage})");
            _ = Broadcast(life, FlatBuffers.Protocol.Response.Damaged.Bytes(life.Sequence.Value, damage), exceptSelf: false, sector: life.Sector);
        }

        public void OnHealed(Life life, int heal)
        {
            if (life.Sequence == null)
                return;

            Log.Logger.Information($"heal : {life.Sequence.Value}({heal})");
            _ = Broadcast(life, FlatBuffers.Protocol.Response.Healed.Bytes(life.Sequence.Value, heal), exceptSelf: false, sector: life.Sector);
        }

        public void OnEquipmentChanged(Character character, EquipmentType equipmentType)
        {
            _ = Broadcast(character, FlatBuffers.Protocol.Response.Character.Bytes(character.ToProtocol()), exceptSelf: false, sector: character.Sector);
        }

        public void OnItemAdded(Character character, Item item)
        {
            _ = character.Send(FlatBuffers.Protocol.Response.ItemAdd.Bytes(item.Name));
        }

        public void OnItemRemoved(Character character, Item item)
        {
            _ = character.Send(FlatBuffers.Protocol.Response.ItemRemove.Bytes(item.Name));
        }

        public void OnPositionChanged(Object obj)
        {
            if (obj.Type == ObjectType.Mob)
            {
                var mob = obj as Mob;
                if (mob.Sector == null)
                    return;

                var mobCase = MasterData.MasterTable.From<MasterData.Table.TableMob>()[mob.Name] ??
                    throw new System.Exception("블라잉블라잉");

                var damage = mobCase.Damage;
                foreach (var character in mob.Sector.Nears.SelectMany(x => x.Characters).Where(x => x.CollisionBox.Contains(mob.CollisionBox)))
                    character.Hp -= damage;
            }
        }

        public void OnLevelChanged(Character character, int before, int after)
        {
            _ = Broadcast(character, FlatBuffers.Protocol.Response.LevelUp.Bytes(character.Sequence.Value, after), exceptSelf: false, sector: character.Sector);
        }

        public void OnExpChanged(Character character, long before, long after)
        {
            _ = character.Send(FlatBuffers.Protocol.Response.ExpChange.Bytes(after));
        }
    }
}
