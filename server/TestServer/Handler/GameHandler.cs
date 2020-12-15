using NetworkShared;
using NetworkShared.Table;
using ServerShared.Model;
using ServerShared.NetworkHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TestServer.Model;

namespace TestServer.Handler
{
    public partial class GameHandler : BaseHandler<Character>
    {
        private static readonly Lazy<GameHandler> _instance = new Lazy<GameHandler>(() => new GameHandler());
        public static GameHandler Instance => _instance.Value;

        private Dictionary<string, Map> _maps;
        private Dictionary<string, NPC> _npcs = new Dictionary<string, NPC>();
        public List<Session<Character>> _movingSessions = new List<Session<Character>>();

        private GameHandler()
        {
            _maps = Map.Load(Directory.GetFiles("Resources/Map", "*.json")); ;

            // NPC spawn
            foreach (var (id, x) in MasterTable.From<TableNpc>())
            {
                var map = _maps[x.Map] ??
                    throw new Exception($"{x.Map} is not valid map name.");

                var createdNPC = new NPC
                {
                    Name = id,
                    Script = x.Script,
                    Listener = this,
                };

                _npcs.Add(id, createdNPC);
                map.Add(createdNPC, x.Position);
            }
        }

        private void Synchronize(DateTime now)
        {
            _movingSessions.ForEach(x => x.Data?.Synchronize(now));
        }

        public async Task Broadcast(Character pivot, byte[] bytes, bool exceptSelf = true)
        {
            var targets = pivot.Map.Sectors.Nears(pivot.Position).SelectMany(x => x.Characters);

            if (exceptSelf)
                targets = targets.Except(new[] { pivot });

            foreach (var target in targets)
            {
                await target.Context.Send(bytes);
            }
        }

        public async Task Broadcast(Map map, byte[] bytes)
        {
            foreach (var context in map.Sectors.SelectMany(x => x.Characters).Select(x => x.Context))
            {
                await context.Send(bytes);
            }
        }

        protected override void OnConnected(Session<Character> session)
        {
            var mapFirst = _maps.First().Value;

            session.Data.Listener = this;
            session.Data.Context = session;
            session.Data.Name = $"{Guid.NewGuid()}";
            mapFirst.Add(session.Data);
        }

        protected override void OnDisconnected(Session<Character> session)
        {
            _movingSessions.Remove(session);

            var character = session.Data;
            character.Map.Remove(character);
        }
    }
}
