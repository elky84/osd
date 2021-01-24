using KeraLua;
using NetworkShared;
using NetworkShared.Types;
using System;

namespace TestServer.Model
{
    public abstract class Object : ILuable
    {
        private Point _position;

        public interface IListener
        {
            public void OnLeave(Object obj);
            public void OnEnter(Object obj);
            public void OnSectorChanged(Object obj);
        }
        public IListener Listener { get; set; }

        public abstract ObjectType Type { get; }
        public Direction Direction { get; set; } = Direction.Bottom;

        public virtual string Name { get; set; }
        public Point Position
        {
            get => _position;
            set
            {
                _position = value;
                UpdatedPositionTime = DateTime.Now;
                MaxJumpY = value.Y;
            }
        }
        public Point Velocity { get; set; } = new Point();
        public DateTime UpdatedPositionTime { get; private set; } = DateTime.Now;
        public double MaxJumpY = 0;
        

        public bool Jumping => (int)Velocity.Y != 0;
        public bool Falling => (int)Velocity.Y > 0;
        public bool Moving => (int)Velocity.X != 0;


        public static implicit operator FlatBuffers.Protocol.Response.Object.Model(Object obj) =>
            new FlatBuffers.Protocol.Response.Object.Model(obj.Sequence.Value, obj.Name, (int)obj.Type, obj.Position, obj.Moving, (int)obj.Direction);

        public static implicit operator FlatBuffers.Protocol.Response.Show.Model(Object obj) =>
            new FlatBuffers.Protocol.Response.Show.Model(obj.Sequence.Value, obj.Name, obj.Position, obj.Moving, (int)obj.Direction);

        public static implicit operator FlatBuffers.Protocol.Response.State.Model(Object obj) =>
            new FlatBuffers.Protocol.Response.State.Model(obj.Sequence.Value, obj.Position, obj.Velocity, (int)obj.Direction, obj.Jumping);

        private Map _map;
        public Map Map
        {
            get => _map;
            set
            {
                if (_map == value)
                    return;

                if (_map != null)
                {
                    Listener?.OnLeave(this);
                    _map.Remove(this);
                }

                _map = value;
                if (value != null)
                {
                    _map.Add(this);
                    Listener?.OnEnter(this);
                }
            }
        }
        public int? Sequence { get; set; }

        private Map.Sector _sector;
        public Map.Sector Sector
        {
            get => _sector;
            set
            {
                if (_sector == value)
                    return;

                _sector = value;
                Listener?.OnSectorChanged(this);
            }
        }
        public virtual bool IsActive => true;

        public bool ValidPosition(Point position)
        {
            var elapsed = (DateTime.Now - UpdatedPositionTime).TotalMilliseconds;
            var diff = new Point(position.X - Position.X, position.Y - Position.Y);

            var calculatedX = (elapsed * Velocity.X) / 1000.0;
            if (diff.X > calculatedX)
                return false;

            if (Jumping)
            {
                // TODO: 점프할 때 최대위치 계산하고 그 위치보다 높은 곳에 있으면 false
                if (MaxJumpY < position.Y)
                    return false;
            }
            else
            {
                var calculatedY = (elapsed * 10.0) / 1000.0;
                if (diff.Y > calculatedY)
                    return false;
            }

            return true;
        }

        public void BindEvent(IListener listener)
        {
            Listener = listener;
        }

        public static int BuiltinName(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var obj = lua.ToLuable<Object>(1);

            lua.PushString(obj.Name);
            return 1;
        }

        public static int BuiltinPosition(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var obj = lua.ToLuable<Object>(1);

            lua.PushNumber(obj.Position.X);
            lua.PushNumber(obj.Position.Y);
            return 2;
        }

        public static int BuiltinMap(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var obj = lua.ToLuable<Object>(1);

            lua.PushLuable(obj.Map);
            return 1;
        }
    }
}
