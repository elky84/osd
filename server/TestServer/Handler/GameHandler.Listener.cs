using FlatBuffers.Protocol;
using ServerShared.NetworkHandler;
using System;
using System.Linq;
using TestServer.Model;

namespace TestServer.Handler
{
    public partial class GameHandler : Model.Object.IListener
    {
        public void OnLeave(Model.Object obj)
        {
            var character = obj as Character;
            if (character == null)
                return;

            _ = Broadcast(character, Leave.Bytes(obj.Sequence.Value));
        }

        public void OnEnter(Model.Object obj)
        {
            var character = obj as Character;
            if (character == null)
                return;

            // 현재 맵에 있는 모든 오브젝트
            var objects = character.Map.Objects
                .Select(x => new FlatBuffers.Protocol.Object.Model(x.Value.Name, x.Key, (int)x.Value.Type, new FlatBuffers.Protocol.Position.Model(x.Value.Position.X, x.Value.Position.Y)))
                .ToList();

            // 현재 맵의 모든 포탈
            var portals = character.Map.Portals.ConvertAll(x => new FlatBuffers.Protocol.Portal.Model(new FlatBuffers.Protocol.Position.Model { X = x.BeforePosition.X, Y = x.BeforePosition.Y }, x.AfterMap));

            // 입장한 유저에게 정보 전송
            _ = character.Context.Send(Enter.Bytes(character.Sequence.Value,
                new FlatBuffers.Protocol.Position.Model(character.Position.X, character.Position.Y),
                new FlatBuffers.Protocol.Map.Model(character.Map.Name),
                objects,
                portals));

            // 기존 유저들에게 정보 전송
            _ = Broadcast(character, Show.Bytes(character.Name, character.Sequence.Value, new FlatBuffers.Protocol.Position.Model { X = character.Position.X, Y = character.Position.Y }));
        }

        public void OnSectorChanged(Model.Object obj)
        {
            Console.WriteLine($"Sector changed({obj.Sequence}, sector : {obj.Sector.Id})");
        }
    }
}
