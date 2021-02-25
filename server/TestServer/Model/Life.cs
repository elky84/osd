using KeraLua;
using NetworkShared.Types;
using System;

namespace TestServer.Model
{
    public abstract class Life : Object
    {
        // listener
        public new interface IListener : Object.IListener
        {
            public void OnDamaged(Life life, int damage);
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
                var damage = (value < _hp) ? _hp - value : 0;

                _hp = Math.Max(0, value);
                if (damage > 0)
                    Listener?.OnDamaged(this, damage);

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

        public abstract int BaseHP { get; }


        // override 
        public override bool IsActive => IsAlive;


        // build-in functions
        public static int BuiltinHp(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var argc = lua.GetTop();
            var life = lua.ToLuable<Life>(1);
            var value = argc < 2 ? null : new double?(lua.ToNumber(2));

            if (value != null)
            {
                life.Hp = (int)value.Value;
                return 0;
            }
            else
            {
                lua.PushInteger(life.Hp);
                return 1;
            }
        }

        public static int BuiltinHpAdd(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var life = lua.ToLuable<Life>(1);
            var value = lua.ToNumber(2);

            life.Hp += (int)value;
            lua.PushInteger(life.Hp);
            return 1;
        }

        public static int BuiltinBaseHp(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var life = lua.ToLuable<Life>(1);

            lua.PushInteger(life.BaseHP);
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
