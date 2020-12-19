using KeraLua;
using NetworkShared;
using NetworkShared.Types;
using System;

namespace TestServer.Model
{
    public abstract class Object : ILuable
    {
        public interface IListener
        {
            public void OnLeave(Object obj);
            public void OnEnter(Object obj);
            public void OnSectorChanged(Object obj);
        }
        public IListener Listener { get; set; }

        public abstract ObjectType Type { get; }

        public FlatBuffers.Protocol.Object.Model FlatBuffer =>
            new FlatBuffers.Protocol.Object.Model(Sequence.Value, Name, (int)Type, Position.FlatBuffer);

        public FlatBuffers.Protocol.Show.Model ShowFlatBuffer => new FlatBuffers.Protocol.Show.Model(Sequence.Value, Name, Position.FlatBuffer);

        public virtual string Name { get; set; }
        public Point Position { get; set; } = new Point();

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
