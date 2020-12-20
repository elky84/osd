using FlatBuffers.Protocol;
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
            _ = Broadcast(obj, Leave.Bytes(obj.Sequence.Value));
        }

        public void OnEnter(Model.Object obj)
        {
            // 현재 맵에 있는 모든 오브젝트
            var objects = obj.Map.Objects
                .Select(x => x.Value.FlatBuffer)
                .ToList();

            // 현재 맵의 모든 포탈
            var portals = obj.Map.Portals.ConvertAll(x => x.FlatBuffer);

            // 입장한 유저에게 정보 전송
            if (obj is Character)
            {
                var character = obj as Character;
                _ = character.Context.Send(Enter.Bytes(character.Sequence.Value,
                    character.Position.FlatBuffer,
                    character.Map.FlatBuffer,
                    objects,
                    portals));

                _ = Broadcast(obj, ShowCharacter.Bytes(character.ShowCharacterFlatBuffer));
            }
            else
            {
                _ = Broadcast(obj, Show.Bytes(obj.Sequence.Value, obj.Name, obj.Position.FlatBuffer));
            }


            // 기존 유저들에게 정보 전송

        }

        public void OnSectorChanged(Model.Object obj)
        {
            Console.WriteLine($"Sector changed({obj.Sequence}, sector : {obj.Sector?.Id})");
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
