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
        public Size Size { get; private set; }
        public SectorContainer Sectors { get; private set; }
        public short[,,] Blocks { get; private set; }
        public Dictionary<int, Object> Objects { get; private set; } = new Dictionary<int, Object>();
        public IEnumerable<Character> Characters => Objects.Values.Select(x => x as Character).Where(x => x != null);
        public IEnumerable<Mob> Mobs => Objects.Values.Select(x => x as Mob).Where(x => x != null);
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

        public static implicit operator FlatBuffers.Protocol.Response.Map.Model(Map map) => new FlatBuffers.Protocol.Response.Map.Model(map.Name);

        public bool IsActivated { get; private set; }

        public List<Portal> Portals => MasterTable.From<TablePortal>().Nears(Name);

        public Dictionary<MobSpawn, List<Mob>> MobSpawns { get; private set; }

        public List<Object> Nears(Point pivot, double distance)
        {
            return Objects.Values
                .Select(x => new { Distance = pivot.Distance(x.Position), Object = x })
                .OrderBy(x => x.Distance)
                .Where(x => x.Distance < distance)
                .Select(x => x.Object)
                .ToList();
        }

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
            var argc = lua.GetTop();
            var map = lua.ToLuable<Map>(1);
            var type = argc < 2 ? null : new long?(lua.ToInteger(2));
            var pivot = argc < 3 ? null : lua.ToLuable<Object>(3);
            var bounds = (pivot == null || argc < 3) ? null : new long?(lua.ToInteger(4));

            lua.NewTable();
            var nears = (pivot != null && bounds != null) ?
                map.Nears(pivot.Position, bounds.Value) :
                map.Objects.Values.ToList();

            foreach (var (obj, i) in nears.Select((x, i) => (x, i)))
            {
                if (type != null && obj.Type != (NetworkShared.ObjectType)type.Value)
                    continue;

                switch (obj.Type)
                {
                    case NetworkShared.ObjectType.Character:
                        lua.PushLuable(obj as Character);
                        break;

                    case NetworkShared.ObjectType.Item:
                        lua.PushLuable(obj as Item);
                        break;

                    case NetworkShared.ObjectType.Mob:
                        lua.PushLuable(obj as Mob);
                        break;

                    case NetworkShared.ObjectType.NPC:
                        lua.PushLuable(obj as NPC);
                        break;

                    default:
                        continue;
                }

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

            Blocks = new short[_format.Layers.Count, _format.Height, _format.Width];
            for (int i = 0; i < _format.Layers.Count; i++)
            {
                var data = _format.Layers[i].Data.ToArray();
                for (int row = 0; row < _format.Height; row++)
                {
                    for (int col = 0; col < _format.Height; col++)
                    {
                        Blocks[i, _format.Height - row - 1, col] = data[row * _format.Width + col];
                    }
                }
            }

            Name = master.Id;
            if (string.IsNullOrEmpty(Name))
                throw new Exception("map name cannot be null or empty.");

            Size = new Size(_format.Width, _format.Height);

#if DEBUG
            Sectors = new SectorContainer(this, new Size(10, 10));
#else
            Sectors = new SectorContainer(this, new Size(800, 800));
#endif

            MobSpawns = mobSpawns.ToDictionary(x => x, x =>
            {
                var mobs = new List<Mob>();
                for (int i = 0; i < x.Count; i++)
                {
                    var mobCase = MasterTable.From<TableMob>()[x.Mob];
                    mobs.Add(new Mob(mobCase, this));
                }

                return mobs;
            });
        }

        private bool Collision(int row, double x, double size, int layer = 0)
        {
            if (row >= Size.Width)
                return false;

            var beginX = Math.Clamp((int)(x - size / 2.0), 0, Size.Width);
            var endX = Math.Clamp((int)(x + size / 2.0), 0, Size.Height);

            for (int col = beginX; col <= endX; col++)
            {
                if (Blocks[layer, row, col] > 0)
                    return true;
            }

            return false;
        }

        public int Collision(Point position, SizeF size, int layer = 0)
        {
            for (int row = (int)(position.Y + size.Height / 2.0); row >= 0; row--)
            {
                if (Collision(row, position.X, size.Width, layer))
                    return row;
            }

            return -1;
        }

        public Point ToGround(Point position, SizeF size, int layer = 0)
        {
            var adjustedX = (int)(position.X - size.Width / 2.0) + (size.Width / 2.0);
            var row = Collision(new Point { X = adjustedX, Y = position.Y }, size, layer);
            return new Point { X = adjustedX, Y = row + 1 + size.Height / 2.0 };
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
                        endPoint = new Point { X = Size.Width, Y = Size.Height };

                    var randomPoint = new Point { X = random.Next((int)beginPoint.X, (int)endPoint.X), Y = random.Next((int)beginPoint.Y, (int)endPoint.Y) };
                    //var groundPoint = this.ToGround(randomPoint, new SizeF { Width = 0.6f, Height = 1.2f });
                    //unspawned.Spawn(this, groundPoint);
                    unspawned.Spawn(this, randomPoint);
                }
            }
        }

        //public byte Block(Point position, uint layer = 0)
        //{
        //    var offsetX = Math.Clamp((int)(position.X / TileSize.Width), 0, BlockSize.Width);
        //    var offsetY = Math.Clamp((int)(position.Y / TileSize.Height), 0, BlockSize.Height);

        //    layer = Math.Max(0, layer);
        //    if (layer > _format.Layers.Count)
        //        throw new Exception($"Layer cannot be {layer}. (only available between 0 and {_format.Layers.Count})");

        //    var selectedLayer = _format.Layers[(int)layer];
        //    var offsetBlock = offsetY * BlockSize.Width + offsetX;
        //    if (offsetBlock > selectedLayer.Data.Count)
        //        throw new Exception($"block offset cannot be {offsetBlock}. (only available bettwen 0 and {selectedLayer.Data.Count})");

        //    return selectedLayer.Data[offsetBlock];
        //}
    }
}
