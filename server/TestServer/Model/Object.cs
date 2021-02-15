using KeraLua;
using NetworkShared;
using NetworkShared.Types;
using System;

namespace TestServer.Model
{
    public abstract class Object : ILuable
    {
        public static double GRAVITY = -9.81;
        public static double GRAVITY_LIMIT = -20.0;

        private Point _position = new Point();

        public interface IListener
        {
            public void OnLeave(Object obj);
            public void OnEnter(Object obj);
            public void OnSectorChanged(Object obj, Map.Sector sector1, Map.Sector sector2);
            public void OnSectorEntering(Object obj, Map.Sector sector);
            public void OnSectorEntered(Object obj, Map.Sector sector);
            public void OnSectorLeaving(Object obj, Map.Sector sector);
            public void OnSectorLeaved(Object obj, Map.Sector sector);
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
                this._position = value;
                this.UpdatedPositionTime = DateTime.Now;

                if (this._map != null)
                    this._map.Update(this);
            }
        }
        public Point Velocity
        {
            get
            {
                var x = 0.0;
                if (this.Moving)
                {
                    x = this.Direction switch
                    {
                        Direction.Left => this.Speed * -1,
                        Direction.Right => this.Speed,
                        _ => 0,
                    };
                }

                var y = 0.0;
                if (Jumping)
                    y = GRAVITY_LIMIT;

                return new Point(x, y);
            }
        }
        public DateTime UpdatedPositionTime { get; private set; } = DateTime.Now;
        
        public static readonly double BaseSpeed = 1.0;
        public double SpeedPercentage { get; private set; } = 1.0;
        public double Speed => BaseSpeed * this.SpeedPercentage;
        public bool Moving { get; private set; } = false;
        public double VelocityX
        {
            get
            {
                return this.Direction switch
                {
                    Direction.Left => this.Speed * -1,
                    Direction.Right => this.Speed,
                    _ => 0,
                };
            }
        }

        public static readonly double BaseJumpingPower = 30.0;
        public double JumpingPowerPercentage { get; private set; } = 1.0;
        public double JumpingPower => BaseJumpingPower * this.JumpingPowerPercentage;
        public double JumpingLimit { get; private set; }
        public bool Jumping { get; private set; } = false;


        public static implicit operator FlatBuffers.Protocol.Response.Object.Model(Object obj) =>
            obj.ToProtocol();

        public FlatBuffers.Protocol.Response.Object.Model ToProtocol() => new FlatBuffers.Protocol.Response.Object.Model(this.Sequence.Value, this.Name, (int)this.Type, this.Position, this.Moving, (int)this.Direction);

        public FlatBuffers.Protocol.Response.State.Model State(bool jump)
        {
            return new FlatBuffers.Protocol.Response.State.Model(this.Sequence.Value, this.Position, this.Velocity, (int)this.Direction, jump, this.Jumping, this.Moving);
        }

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

                var before = this._sector;
                Listener?.OnSectorLeaving(this, before);
                Listener?.OnSectorEntering(this, value);
                this._sector = value;
                Listener?.OnSectorLeaved(this, before);
                Listener?.OnSectorEntered(this, this._sector);
                Listener?.OnSectorChanged(this, before, value);
            }
        }
        public virtual bool IsActive => true;

        public bool ValidPosition(Point position)
        {
            var elapsed = (DateTime.Now - UpdatedPositionTime).Ticks;
            var diff = new Point(position.X - Position.X, position.Y - Position.Y);

            var calculatedX = (elapsed * this.Velocity.X) / 1000000.0;
            var calculatedDiffX = Math.Abs(diff.X - calculatedX);
            if (calculatedDiffX > 5.0 && calculatedDiffX > calculatedX * 0.025)
                return false;

            if (Jumping == false)
            {
                if (Math.Abs(position.Y - Position.Y) > 1)
                    return false;
            }
            else if (position.Y > Position.Y && Math.Abs(position.Y - Position.Y) > 5.0)
            {
                if (this.JumpingLimit < position.Y)
                    return false;
            }
            else
            {
                var calculatedY = (elapsed * GRAVITY_LIMIT) / 1000000.0;
                if (Math.Abs(diff.Y - calculatedY) > 5.0 && diff.Y < calculatedY)
                    return false;
            }

            return true;
        }

        public void BindEvent(IListener listener)
        {
            Listener = listener;
        }

        public void Move(Direction direction)
        {
            this.Direction = direction;
            this.Moving = true;
        }

        public void Stop()
        {
            this.Moving = false;
        }

        public void Jump(bool enable)
        {
            if (enable)
            {
                if (this.Jumping)
                    throw new Exception("점프중");

                var time = -this.JumpingPower / GRAVITY;
                var top = (this.JumpingPower * time) + (GRAVITY * Math.Pow(time, 2) / 2);
                this.JumpingLimit = this.Position.Y + top;
                Console.WriteLine(this.JumpingLimit);
                this.Jumping = true;
            }
            else
            {
                this.Jumping = false;
            }
        }

        public void Fall()
        {
            this.Jumping = true;
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
