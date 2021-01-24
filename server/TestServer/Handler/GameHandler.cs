using KeraLua;
using MasterData;
using MasterData.Table;
using ServerShared.Model;
using ServerShared.NetworkHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestServer.Factory;
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

        private GameHandler()
        {
            foreach (var (name, func) in Static.BindGlobalBuiltinFunctions<GameHandler>())
                Console.WriteLine($"{name} 함수가 전역으로 등록되었습니다.");

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

                var createdNPC = new NPC(x)
                {
                    Listener = this,
                };

                _npcs.Add(id, createdNPC);
                createdNPC.Position = x.Position;
                createdNPC.Map = map;
            }

            SetTimer(1000, OnRezen);
            ExecuteScheduler();
        }

        private void OnRezen(long ms)
        {
            //Console.WriteLine($"ms : {ms}");
            foreach (var map in _maps.Values.Where(x => x.IsActivated))
                map.Zen();
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

            // 아이템 정보를 전달
            session.Data.Items.Inventory.Add(ItemFactory.Create("무기.검"));
            session.Data.Items.Inventory.Add(ItemFactory.Create("무기.활"));
            session.Data.Items.Inventory.Add(ItemFactory.Create("무기.지팡이"));
            session.Data.Items.Inventory.Add(ItemFactory.Create("장비.옷"));
            _ = session.Send(FlatBuffers.Protocol.Response.Items.Bytes(session.Data.Items.Inventory.SelectMany(x => x.Value).Select(x => (FlatBuffers.Protocol.Response.Item.Model)x).ToList(),
                session.Data.Items.Equipments.Values.Where(x => x != null).Select(x => (FlatBuffers.Protocol.Response.Equipment.Model)x).ToList()));

            session.Data.Map = mapFirst;

            var collision = MasterTable.From<TableCollision>()["캐릭터"];
            session.Data.CollisionSize = new NetworkShared.Types.Size { Width = collision.Width, Height = collision.Height };
        }

        protected override void OnDisconnected(Session<Character> session)
        {
            //_movingSessions.Remove(session);

            var character = session.Data;
            character.Map = null;
        }

        public static int BuiltinMkitem(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var name = lua.ToString(1);

            var item = ItemFactory.Create(name);
            if (item == null)
                lua.PushNil();
            else
                lua.PushLuable(item, item.GetType());

            return 1;
        }
    }
}