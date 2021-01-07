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
        private MapData _format;

        public string Name { get; private set; }
        public Size BlockSize { get; private set; }
        public Size TileSize { get; private set; }
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

        public bool IsActivated { get; private set; }

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

            lua.PushInteger(map.BlockSize.Width);
            lua.PushInteger(map.BlockSize.Height);
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
            _format = JsonConvert.DeserializeObject<MapData>(contents);

            Name = master.Id;
            if (string.IsNullOrEmpty(Name))
                throw new Exception("map name cannot be null or empty.");

            BlockSize = new Size { Width = _format.Width, Height = _format.Height };
            if (BlockSize.IsEmpty)
                throw new Exception($"map size cannot be empty : {Name}.");

            TileSize = new Size { Width = _format.TileWidth, Height = _format.TileHeight };
            if (TileSize.IsEmpty)
                throw new Exception($"map tile size cannot be empty : {Name}.");

            Size = new Size { Width = BlockSize.Width * TileSize.Width, Height = BlockSize.Height * TileSize.Height };

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

        private bool UpdateState()
        {
            IsActivated = Objects.Values.FirstOrDefault(x => x.Type == NetworkShared.ObjectType.Character) != null;
            return IsActivated;
        }

        public Sector Add(Object obj)
        {
            var sequence = NextSequence ??
                throw new Exception("cannot get next sequence.");

            Objects.Add(sequence, obj);
            obj.Sequence = sequence;
            obj.Sector = Sectors.Add(obj);

            if (IsActivated == false && obj.Type == NetworkShared.ObjectType.Character)
                IsActivated = true;

            return obj.Sector;
        }

        public Sector Remove(Object obj)
        {
            if (obj.Sequence.HasValue == false)
                return null;

            Objects.Remove(obj.Sequence.Value);
            UpdateState();
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
                        endPoint = new Point { X = BlockSize.Width, Y = BlockSize.Height };

                    var randomPoint = new Point { X = random.Next((int)beginPoint.X, (int)endPoint.X), Y = random.Next((int)beginPoint.Y, (int)endPoint.Y) };
                    unspawned.Spawn(this, randomPoint);
                }
            }
        }

        public byte Block(Point position, uint layer = 0)
        {
            var offsetX = Math.Clamp((int)(position.X / TileSize.Width), 0, BlockSize.Width);
            var offsetY = Math.Clamp((int)(position.Y / TileSize.Height), 0, BlockSize.Height);

            layer = Math.Max(0, layer);
            if (layer > _format.Layers.Count)
                throw new Exception($"Layer cannot be {layer}. (only available between 0 and {_format.Layers.Count})");

            var selectedLayer = _format.Layers[(int)layer];
            var offsetBlock = offsetY * BlockSize.Width + offsetX;
            if (offsetBlock > selectedLayer.Data.Count)
                throw new Exception($"block offset cannot be {offsetBlock}. (only available bettwen 0 and {selectedLayer.Data.Count})");

            return selectedLayer.Data[offsetBlock];
        }
    }
}