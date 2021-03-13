using KeraLua;
using MasterData.Server;
using NetworkShared;
using NetworkShared.Types;
using System;
using NetworkShared.Common;

namespace TestServer.Model
{
    public class Mob : Life
    {
        public new interface IListener : Life.IListener
        {
            public void OnSpawned(Mob mob);
        }
        public new IListener Listener { get; private set; }

        public override ObjectType Type => ObjectType.Mob;

        public override string Name => Master.Id;

        public override int Hp
        {
            get => base.Hp;
            protected set
            {
                base.Hp = value;
                if (Math.Max(0, value) == 0)
                {
                    SpawnedTime = null;
                }
            }
        }

        public MasterData.Server.Mob Master { get; private set; }

        public int Exp => Master.Expereicen;

        public DateTime? SpawnedTime { get; private set; }

        public bool IsSpawned => SpawnedTime != null;

        public DateTime LastActionDateTime { get; set; } = DateTime.MinValue;

        private Character _owner;
        public Character Owner
        {
            get
            {
                return _owner;
            }
            set
            {
                _owner = value;
                UpdatedPositionTime = DateTime.Now;
            }
        }

        public Mob(MasterData.Server.Mob master)
        {
            Master = master;
        }

        public Mob(MasterData.Server.Mob master, Map map)
        {
            Master = master;
            Map = map;
        }

        public bool Spawn(Map map, Point position)
        {
            if (IsSpawned)
                return false;

            Stats.Base.Set(Master.BaseStat());
            Hp = Stats.Max[StatType.HP];
            SpawnedTime = DateTime.Now;
            Position = position;
            Map = map;

            Listener?.OnSpawned(this);
            return true;
        }

        public void BindEvent(IListener listener)
        {
            base.BindEvent(listener);
            Listener = listener;
        }

        public static int BuiltinExp(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var mob = lua.ToLuable<Mob>(1);

            lua.PushInteger(mob.Exp);
            return 1;
        }
    }
}
