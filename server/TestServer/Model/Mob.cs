using NetworkShared;
using NetworkShared.Types;
using System;

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
            set
            {
                base.Hp = value;
                if (Math.Max(0, value) == 0)
                {
                    SpawnedTime = null;
                    Map = null;
                }
            }
        }

        public MasterData.Table.Mob Master { get; private set; }

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
#if DEBUG
                if (_owner != null)
                    Console.WriteLine($"new owner : {this.Sequence} > {_owner.Sequence}");
                else
                    Console.WriteLine($"unset owner : {this.Sequence}");
#endif
            }
        }

        public Mob(MasterData.Table.Mob master)
        {
            Master = master;
        }

        public bool Spawn(Map map, Point position)
        {
            if (IsSpawned)
                return false;

            Hp = Master.HP;
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
    }
}
