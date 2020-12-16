using MasterData;
using MasterData.Table;
using ServerShared.Model;
using ServerShared.NetworkHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestServer.Model;

namespace TestServer.Handler
{
    public partial class GameHandler : BaseHandler<Character>
    {
        private static readonly Lazy<GameHandler> _instance = new Lazy<GameHandler>(() => new GameHandler());
        public static GameHandler Instance => _instance.Value;

        private Dictionary<string, Model.Map> _maps;
        private Dictionary<string, Model.NPC> _npcs = new Dictionary<string, NPC>();
        private Dictionary<string, Model.Mob> _mobs;
        public List<Session<Character>> _movingSessions = new List<Session<Character>>();

        private GameHandler()
        {
            _mobs = MasterTable.From<TableMob>().Select(x => new Model.Mob(x.Value)).ToDictionary(x => x.Master.Id, x => x);

            _maps = MasterTable.From<TableMap>()
                .Select(x =>
                {
                    var mobSpawns = MasterTable.From<TableMobSpawn>().Where(x => x.Map == x.Map);
                    var map = new Model.Map(x.Value, mobSpawns);
                    foreach (var mob in map.MobSpawns.SelectMany(x => x.Value))
                        mob.BindEvent(this);

                    map.Zen();
                    return map;
                })
                .ToDictionary(x => x.Name);

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
                createdNPC.Position = x.Position;
                createdNPC.Map = map;
            }
        }

        private void Synchronize(DateTime now)
        {
            _movingSessions.ForEach(x => x.Data?.Synchronize(now));
        }

        public async Task Broadcast(Model.Object pivot, byte[] bytes, bool exceptSelf = true, bool inSector = false)
        {
            var targets = inSector ?
                pivot.Map.Sectors.Nears(pivot.Position).SelectMany(x => x.Characters) :
                pivot.Map.Objects.Values.Where(x => x is Character).Select(x => x as Character);

            if (exceptSelf && pivot is Character)
                targets = targets.Except(new[] { pivot as Character });

            foreach (var target in targets)
            {
                await target.Context.Send(bytes);
            }
        }

        public async Task Broadcast(Model.Map map, byte[] bytes)
        {
            foreach (var context in map.Sectors.SelectMany(x => x.Characters).Select(x => x.Context))
            {
                await context.Send(bytes);
            }
        }

        protected override void OnConnected(Session<Character> session)
        {
            var mapFirst = _maps.First().Value;

            session.Data.BindEvent(this);
            session.Data.Context = session;
            session.Data.Name = $"{Guid.NewGuid()}";
            session.Data.Map = mapFirst;
        }

        protected override void OnDisconnected(Session<Character> session)
        {
            _movingSessions.Remove(session);

            var character = session.Data;
            character.Map = null;
        }
    }
}
