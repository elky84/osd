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


        public Size CollisionSize { get; set; }
        public Rect CollisionBox => new Rect
        {
            X = (int)Position.X,
            Y = (int)Position.Y,
            Width = (uint)CollisionSize.Width,
            Height = (uint)CollisionSize.Height
        };


        // properties
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


        // build-in functions
        public static int BuiltinHP(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var life = lua.ToLuable<Life>(1);

            lua.PushInteger(life.Hp);
            return 1;
        }

        // methods

        public void BindEvent(IListener listener)
        {
            base.BindEvent(listener);
            Listener = listener;
        }
    }
}
