using ServerShared.NetworkHandler;
using System;
using System.Linq;
using TestServer.Model;

namespace TestServer.Handler
{
    public partial class GameHandler : Mob.IListener, Character.IListener
    {
        public void OnLeave(Model.Object obj)
        {
            _ = Broadcast(obj, FlatBuffers.Protocol.Response.Leave.Bytes(obj.Sequence.Value));
        }

        public void OnEnter(Model.Object obj)
        {
            // 현재 맵에 있는 모든 오브젝트
            var objects = obj.Map.Objects
                .Select(x => (FlatBuffers.Protocol.Response.Object.Model)x.Value)
                .ToList();

            var portals = obj.Map.Portals
                .ConvertAll(x => (FlatBuffers.Protocol.Response.Portal.Model)x)
                .ToList();

            // 입장한 유저에게 정보 전송
            if (obj is Character)
            {
                var character = obj as Character;
                _ = character.Context.Send(FlatBuffers.Protocol.Response.Enter.Bytes(character.Sequence.Value,
                    character.Map,
                    character.Position,
                    (int)character.Direction,
                    objects,
                    portals));

                _ = Broadcast(obj, FlatBuffers.Protocol.Response.ShowCharacter.Bytes(character));
            }
            else
            {
                _ = Broadcast(obj, FlatBuffers.Protocol.Response.Show.Bytes(obj.Sequence.Value, obj.Name, obj.Position, obj.Moving, (int)obj.Direction));
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
    }
}
