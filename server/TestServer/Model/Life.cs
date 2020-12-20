using KeraLua;
using NetworkShared;
using NetworkShared.Types;
using System;

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
        public uint Speed { get; set; } = 10;
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


        // methods
        public Point Synchronize(DateTime time)
        {
            if (MoveTime == null)
                return Position;

            var diff = time - MoveTime.Value;
            var moved = diff.TotalMilliseconds * (Speed / 1000.0);

            switch (Direction)
            {
                case Direction.Left:
                    Position.X -= moved;
                    break;

                case Direction.Top:
                    Position.Y += moved;
                    break;

                case Direction.Right:
                    Position.X += moved;
                    break;

                case Direction.Bottom:
                    Position.Y -= moved;
                    break;

                default:
                    throw new Exception("Invalid direction value");
            }

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
