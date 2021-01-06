using KeraLua;
using NetworkShared.Types;
using System;
using System.Numerics;

namespace TestServer.Model
{
    public abstract class Life : Object
    {
        // listener
        public new interface IListener : Object.IListener
        {
            public void OnDie(Life life);
        }
        public new IListener Listener { get; private set; }


        // properties
        public DateTime? MoveTime { get; set; }

        public Vector2 Velocity { get; set; } = new Vector2();
        public bool IsAlive { get; set; }
        public DateTime? DeadTime { get; private set; }
        public void Kill() => Hp = 0;


        // virtual
        private int _hp = 50;
        public virtual int Hp
        {
            get => _hp;
            set
            {
                _hp = Math.Max(0, value);
                if (_hp > 0)
                {
                    IsAlive = true;
                    DeadTime = null;
                }
                else
                {
                    IsAlive = false;
                    DeadTime = DateTime.Now;
                    Listener?.OnDie(this);
                }
            }
        }


        // override 
        public override bool IsActive => IsAlive;
        public override bool Moving => MoveTime.HasValue;


        // build-in functions
        public static int BuiltinHP(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var life = lua.ToLuable<Life>(1);

            lua.PushInteger(life.Hp);
            return 1;
        }

        public Point Synchronize(float elapsedMilliseconds)
        {
            var before = new Vector2((float)Position.X, (float)Position.Y);
            var moved = Vector2.Multiply(Velocity, elapsedMilliseconds / 1000.0f);
            var after = Vector2.Add(before, moved);

            Position = new Point(after.X, after.Y);
            return Position;
        }


        // methods
        public Point Synchronize(DateTime time)
        {
            if (MoveTime == null)
                return Position;

            Synchronize((float)(time - MoveTime.Value).TotalMilliseconds);

            Map.Update(this);
            return Position;
        }

        public void BindEvent(IListener listener)
        {
            base.BindEvent(listener);
            Listener = listener;
        }
    }
}
