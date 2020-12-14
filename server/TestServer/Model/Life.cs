using KeraLua;
using NetworkShared;
using NetworkShared.Types;
using System;

namespace TestServer.Model
{
    public class Life : Object
    {
        public int Hp { get; set; } = 50;
        public Direction Direction { get; set; }
        public DateTime? Time { get; set; }
        public uint Speed { get; set; } = 10;

        public static int BuiltinHP(IntPtr luaState)
        {
            var lua = Lua.FromIntPtr(luaState);
            var life = lua.ToLuable<Life>(1);

            lua.PushInteger(life.Hp);
            return 1;
        }

        public Point Synchronize(DateTime time)
        {
            if (Time == null)
                return Position;

            var diff = time - Time.Value;
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
    }
}
