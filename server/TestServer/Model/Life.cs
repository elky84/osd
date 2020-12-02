using KeraLua;
using System;
using System.Drawing;

namespace TestServer.Model
{
    public enum Direction
    {
        Left, Top, Right, Bottom
    }

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

        public Point UpdatePosition(DateTime time)
        {
            if (Time == null)
                return Position;

            var diff = time - Time.Value;
            var moved = (int)(diff.TotalMilliseconds * (Speed / 1000.0));

            switch (Direction)
            {
                case Direction.Left:
                    Position = new Point(Position.X - moved, Position.Y);
                    break;

                case Direction.Top:
                    Position = new Point(Position.X, Position.Y + moved);
                    break;

                case Direction.Right:
                    Position = new Point(Position.X + moved, Position.Y);
                    break;

                case Direction.Bottom:
                    Position = new Point(Position.X, Position.Y - moved);
                    break;

                default:
                    throw new Exception("Invalid direction value");
            }

            return Position;
        }
    }
}
