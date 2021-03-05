using KeraLua;
using MasterData;
using MasterData.Table;
using NetworkShared;
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
            SetTimer(100, OnMobAction);
            ExecuteScheduler();
        }

        private void OnMobAction(long ms)
        {
            var now = DateTime.Now;
            var random = new Random((int)now.Ticks);
            foreach (var map in _maps.Where(x => x.Value.IsActivated))
            {
                var mobs = map.Value.Mobs
                    .Where(x => x.Sector.Nears.Any(x => x.Activated))
                    .Where(x =>
                    {
                        var elapsed = now - x.LastActionDateTime;
                        return elapsed.TotalMilliseconds > x.Master.Speed * 1000;
                    });

                foreach (var mob in mobs)
                {
                    var direction = (Direction)random.Next(2);
                    switch (direction)
                    {
                        case Direction.Left:
                            _ = Broadcast(mob, FlatBuffers.Protocol.Response.Action.Bytes(mob.Sequence.Value, (int)ActionPattern.LeftMove), sector: mob.Sector);
                            break;

                        case Direction.Right:
                            _ = Broadcast(mob, FlatBuffers.Protocol.Response.Action.Bytes(mob.Sequence.Value, (int)ActionPattern.RightMove), sector: mob.Sector);
                            break;
                    }

                    mob.LastActionDateTime = now;
                }
            }
        }

        private void OnRezen(long ms)
        {
            //Console.WriteLine($"ms : {ms}");
            foreach (var map in _maps.Values.Where(x => x.IsActivated))
                map.Zen();
        }

        public async Task Broadcast(Model.Object pivot, byte[] bytes, bool exceptSelf = true, Model.Map.Sector sector = null)
        {
            if (pivot.Map == null)
                return;

            var targets = sector != null ?
                sector.Nears.SelectMany(x => x.Characters) :
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

        private Model.Object GetControllableObject(Session<Character> session, int sequence)
        {
            var character = session.Data;
            if (character.Map.Objects.TryGetValue(sequence, out var obj) == false)
                return null;

            switch (obj.Type)
            {
                case ObjectType.Character:
                    {
                        if (sequence != character.Sequence)
                            return null;
                    }
                    break;

                case ObjectType.Mob:
                    {
                        var mob = obj as Model.Mob;
                        if (mob.Owner != character)
                            return null;
                    }
                    break;

                default:
                    return null;
            }

            return obj;
        }

        protected override void OnConnected(Session<Character> session)
        {
            var mapFirst = _maps.First().Value;

            session.Data.BindEvent(this);
            session.Data.Context = session;
            session.Data.Name = $"{Guid.NewGuid()}";

            // 아이템 정보를 전달
            var weapon = session.Data.Items.Inventory.Add(ItemFactory.Create("무기.검")) as Weapon; ;
            session.Data.Items.Inventory.Add(ItemFactory.Create("무기.활"));
            session.Data.Items.Inventory.Add(ItemFactory.Create("무기.지팡이"));
            session.Data.Items.Inventory.Add(ItemFactory.Create("장비.옷"));
            session.Data.Items.Inventory.Add(ItemFactory.Create("기본신발"));

            var inventory = session.Data.Items.Inventory.SelectMany(x => x.Value).Select(x => (FlatBuffers.Protocol.Response.Item.Model)x).ToList();
            var equipments = session.Data.Items.Equipments.Values.Where(x => x != null).Select(x => (FlatBuffers.Protocol.Response.Equipment.Model)x).ToList();
            _ = session.Send(FlatBuffers.Protocol.Response.Items.Bytes(inventory, equipments, session.Data.Items.Gold));

            //session.Data.Position = mapFirst.ToGround(new NetworkShared.Types.Point(10, 10), new SizeF { Width = 0.6f, Height = 1.2f });
            session.Data.Position = new NetworkShared.Types.Point(10, 10);
            session.Data.Map = mapFirst;

            var collision = MasterTable.From<TableCollision>()["캐릭터"];
            session.Data.CollisionSize = new NetworkShared.Types.SizeF { Width = collision.Width, Height = collision.Height };

            // 무기를 장착
            session.Data.Items.Active(weapon.Id);


            // 스킬을 배움
            foreach (var (name, skill) in MasterTable.From<TableSkill>())
                session.Data.Skills.Add(SkillFactory.Create(session.Data, name));
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