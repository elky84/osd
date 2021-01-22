using System.Linq;

namespace NetworkShared.Types
{
    public class Rect
    {
        public int X { get; set; }
        public int Y { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }

        public int Left => X;
        public int Top => Y;
        public int Right => X + (int)Width;
        public int Bottom => Y + (int)Height;

        public Point LeftTop => new Point { X = Left, Y = Top };
        public Point LeftBottom => new Point { X = Left, Y = Bottom };
        public Point RightTop => new Point { X = Right, Y = Top };
        public Point RightBottom => new Point { X = Right, Y = Bottom };
        public Point[] Points => new[] { LeftTop, LeftBottom, RightTop, RightBottom };

        public bool Contains(Point point)
        {
            return point.X > Left &&
                point.X < Right &&
                point.Y > Top &&
                point.Y < Bottom;
        }

        public bool Contains(Rect area)
        {
            if (Points.Any(x => area.Contains(x)))
                return true;

            if (area.Points.Any(X => Contains(X)))
                return true;

            return false;
        }
    }
}
