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

        public override int Hp
        {
            get => base.Hp;
            set
            {
                if (Math.Max(0, value) == 0)
                    Map = null;

                base.Hp = value;
            }
        }

        public MasterData.Table.Mob Master { get; private set; }

        public DateTime? SpawnedTime { get; private set; }

        public bool IsSpawned => SpawnedTime != null;

        public Mob(MasterData.Table.Mob master)
        {
            Name = master.Id;
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
