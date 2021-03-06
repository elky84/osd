using KeraLua;
using NetworkShared.Types;
using System;
using TestServer.Container;

namespace TestServer.Model
{
    public abstract class Life : Object
    {
        // listener
        public new interface IListener : Object.IListener
        {
            public void OnHealed(Life life, Life from, int heal);
            public void OnDamaged(Life life, Life from, int damage);
            public void OnDie(Life life, Life from);
        }
        public new IListener Listener { get; private set; }


        public SizeF CollisionSize { get; set; }
        public RectF CollisionBox => new RectF
        {
            X = Position.X - CollisionSize.Width / 2.0,
            Y = Position.Y - CollisionSize.Height / 2.0,
            Width = CollisionSize.Width,
            Height = CollisionSize.Height
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
            protected set
            {
                _hp = Math.Clamp(value, 0, Stats.Max[NetworkShared.StatType.HP]);
                if (_hp > 0)
                {
                    IsAlive = true;
                    DeadTime = null;
                }
                else
                {
                    IsAlive = false;
                    DeadTime = DateTime.Now;
                }
            }
        }

        public StatContainer Stats { get; private set; } = new StatContainer();


        // override 
        public override bool IsActive => IsAlive;

        // methods
        public virtual void Damage(int damage, Life from = null)
        {
            this.Hp -= damage;
            Listener?.OnDamaged(this, from, damage);

            if (this.IsAlive == false)
                Listener?.OnDie(this, from);
        }

        public virtual void Heal(int heal, Life from = null)
        {
            this.Hp += heal;
            Listener?.OnHealed(this, from, heal);
        }


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
            var argc = lua.GetTop();
            var life = lua.ToLuable<Life>(1);
            var value = lua.ToNumber(2);

            if (argc == 2)
            {
                life.Hp += (int)value;
            }
            else
            {
                var from = lua.ToLuable<Life>(3);
                if (value < 0)
                    life.Damage((int)-value, from);
                else
                    life.Heal((int)value, from);
            }

            lua.PushInteger(life.Hp);
            return 1;
        }

        public static int BuiltinBaseHp(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var life = lua.ToLuable<Life>(1);

            lua.PushInteger(life.Stats.Max[NetworkShared.StatType.HP]);
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
