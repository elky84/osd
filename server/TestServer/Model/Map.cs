using KeraLua;
using MasterData;
using MasterData.Table;
using NetworkShared.Common;
using NetworkShared.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestServer.Model
{
    public partial class Map : ILuable
    {
        private int _currentSequence = 0;

        public string Name { get; private set; }
        public Size Size { get; private set; }
        public SectorContainer Sectors { get; private set; }
        public Dictionary<int, Object> Objects { get; private set; } = new Dictionary<int, Object>();
        public int? NextSequence
        {
            get
            {
                for (int i = _currentSequence; i < int.MaxValue; i++)
                {
                    if (Objects.ContainsKey(i))
                        continue;

                    _currentSequence = i + 1;
                    return i;
                }

                for (int i = 0; i < _currentSequence; i++)
                {
                    if (Objects.ContainsKey(i))
                        continue;

                    _currentSequence = i + 1;
                    return i;
                }

                return null;
            }
        }

        public FlatBuffers.Protocol.Map.Model FlatBuffer => new FlatBuffers.Protocol.Map.Model(Name);

        public bool IsActivated => Objects.Values.FirstOrDefault(x => x.Type == NetworkShared.ObjectType.Character) != null;

        public List<Portal> Portals => MasterTable.From<TablePortal>().Nears(Name);

        public Dictionary<MobSpawn, List<Mob>> MobSpawns { get; private set; }

        public static int BuiltinName(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var map = lua.ToLuable<Map>(1);

            lua.PushString(map.Name);
            return 1;
        }

        public static int BuiltinSize(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var map = lua.ToLuable<Map>(1);

            lua.PushInteger(map.Size.Width);
            lua.PushInteger(map.Size.Height);
            return 2;
        }

        public static int BuiltinObjects(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var map = lua.ToLuable<Map>(1);

            lua.NewTable();
            foreach (var (obj, i) in map.Objects.Select((x, i) => (x, i)))
            {
                lua.PushLuable(obj.Value);
                lua.RawSetInteger(-2, i + 1);
            }

            return 1;
        }

        public Map(MasterData.Table.Map master, IEnumerable<MobSpawn> mobSpawns)
        {
            if (File.Exists(master.Data) == false)
                throw new Exception($"cannot find map file : {master.Data}");

            var contents = File.ReadAllText(master.Data);
            var format = JsonConvert.DeserializeObject<MapData>(contents);

            Name = format.StageFileName;
            if (string.IsNullOrEmpty(Name))
                throw new Exception("map name cannot be null or empty.");
            Size = new Size { Width = format.MapTileSize.x, Height = format.MapTileSize.y };
            if (Size.IsEmpty)
                throw new Exception($"map size cannot be empty : {Name}.");

#if DEBUG
            Sectors = new SectorContainer(this, new Size { Width = 8, Height = 8 });
#else
            Sectors = new SectorContainer(this, new Size { Width = 64, Height = 64 });
#endif

            MobSpawns = mobSpawns.ToDictionary(x => x, x =>
            {
                var mobs = new List<Mob>();
                for (int i = 0; i < x.Count; i++)
                {
                    var mobCase = MasterTable.From<TableMob>()[x.Mob];
                    mobs.Add(new Mob(mobCase));
                }

                return mobs;
            });
        }

        public Sector Add(Object obj)
        {
            var sequence = NextSequence ??
                throw new Exception("cannot get next sequence.");

            Objects.Add(sequence, obj);
            obj.Sequence = sequence;
            obj.Sector = Sectors.Add(obj);
            return obj.Sector;
        }

        public Sector Remove(Object obj)
        {
            if (obj.Sequence.HasValue == false)
                return null;

            Objects.Remove(obj.Sequence.Value);
            return Sectors.Remove(obj);
        }

        public Sector Update(Object obj)
        {
            if (obj.Sequence.HasValue == false || Objects.ContainsKey(obj.Sequence.Value) == false)
                return null;

            if (obj.Sector == null)
                return null;

            return Sectors.Add(obj);
        }

        public void Zen()
        {
            var random = new Random();
            var now = DateTime.Now;
            foreach (var (spawnCase, mobs) in MobSpawns)
            {
                foreach (var unspawned in mobs.Where(x => x.IsSpawned == false))
                {
                    var elapsedDeadTime = now - (unspawned.DeadTime ?? DateTime.MinValue);
                    if (elapsedDeadTime < spawnCase.ZenTime)
                        continue;

                    var beginPoint = spawnCase.Begin;
                    if (beginPoint == null)
                        beginPoint = new Point { X = 0, Y = 0 };

                    var endPoint = spawnCase.End;
                    if (endPoint == null)
                        endPoint = new Point { X = Size.Width, Y = Size.Height };

                    var randomPoint = new Point { X = random.Next((int)beginPoint.X, (int)endPoint.X), Y = random.Next((int)beginPoint.Y, (int)endPoint.Y) };
                    unspawned.Spawn(this, randomPoint);
                }
            }
        }
    }
}