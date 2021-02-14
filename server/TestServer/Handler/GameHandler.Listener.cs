using ServerShared.NetworkHandler;
using System;
using System.Collections.Generic;
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
                _ = character.Context.Send(FlatBuffers.Protocol.Response.Enter.Bytes(character.ToProtocol(),
                    character.Map,
                    character.Position,
                    (int)character.Direction,
                    portals));
            }
        }

        public void OnSectorChanged(Model.Object obj, Map.Sector sector1, Map.Sector sector2)
        {
            if (obj.Type == NetworkShared.ObjectType.Character)
            {
                var character = obj as Character;
                var befores = sector1 != null ? 
                    sector1.Nears.Where(x => x != null).SelectMany(x => x.Objects) :
                    Enumerable.Empty<Model.Object>();
                var afters = sector2 != null ? 
                    sector2.Nears.Where(x => x != null).SelectMany(x => x.Objects) :
                    Enumerable.Empty<Model.Object>();

                var hides = befores.Except(afters).ToList();
                _ = character.Context.Send(FlatBuffers.Protocol.Response.Leave.Bytes(hides.Select(x => x.Sequence.Value).ToList()));
                foreach (var x in hides.Where(x => x.Type == NetworkShared.ObjectType.Character))
                {
                    var ch = x as Character;
                    _ = ch.Context.Send(FlatBuffers.Protocol.Response.Leave.Bytes(new System.Collections.Generic.List<int> { character.Sequence.Value }));
                }

                var shows = afters.Except(befores).ToList();
                _ = character.Context.Send(FlatBuffers.Protocol.Response.Show.Bytes(
                    shows.Where(x => x.Type != NetworkShared.ObjectType.Character).Select(x => x.ToProtocol()).ToList(),
                    shows.Where(x => x.Type == NetworkShared.ObjectType.Character).Select(x => (x as Character).ToProtocol()).ToList()));
                foreach (var x in shows.Where(x => x.Type == NetworkShared.ObjectType.Character))
                {
                    var ch = x as Character;
                    _ = ch.Context.Send(FlatBuffers.Protocol.Response.Show.Bytes(
                        new System.Collections.Generic.List<FlatBuffers.Protocol.Response.Object.Model> { },
                        new System.Collections.Generic.List<FlatBuffers.Protocol.Response.Character.Model> { character }));
                }

                foreach (var (sector, objs) in hides.GroupBy(x => x.Sector).ToDictionary(x => x.Key, x => x.ToList()))
                {
                    var newOwner = sector.Nears.SelectMany(x => x.Characters).FirstOrDefault();
                    foreach (var mob in objs.Where(x => x.Type == NetworkShared.ObjectType.Mob).Select(x => x as Mob))
                        mob.Owner = newOwner;
                }
                

                foreach (var (sector, objs) in shows.GroupBy(x => x.Sector).ToDictionary(x => x.Key, x => x.ToList()))
                {
                    var oldOwner = sector.Nears.SelectMany(x => x.Characters).FirstOrDefault();
                    foreach (var mob in objs.Where(x => x.Type == NetworkShared.ObjectType.Mob).Select(x => x as Mob))
                        mob.Owner = mob.Owner ?? character;
                }
            }
            else if(obj.Type == NetworkShared.ObjectType.Mob)
            {
                var mob = obj as Mob;
                mob.Owner = mob.Sector.Nears.SelectMany(x => x.Characters).FirstOrDefault();
            }
        }

        public void OnSpawned(Mob mob)
        {
            // 이런 형식으로 쓰면 편하긴 한데 퍼포먼스 이슈가...
            Console.WriteLine($"Spawned '{mob.Name}' in '{mob.Map.Name}' ({mob.Position.X}, {mob.Position.Y})");
        }

        public void OnDie(Life life)
        {
            Console.WriteLine($"{life.Name}({life.Sequence}) is dead.");
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
    }
}
