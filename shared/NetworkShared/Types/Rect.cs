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

    public class RectF
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public double Left => X;
        public double Top => Y;
        public double Right => X + Width;
        public double Bottom => Y + Height;

        public Point LeftTop => new Point { X = Left, Y = Top };
        public Point LeftBottom => new Point { X = Left, Y = Bottom };
        public Point RightTop => new Point { X = Right, Y = Top };
        public Point RightBottom => new Point { X = Right, Y = Bottom };
        public Point[] Points => new[] { LeftTop, LeftBottom, RightTop, RightBottom };

        public bool Contains(Point point)
        {
            return point.X >= Left &&
                point.X <= Right &&
                point.Y >= Top &&
                point.Y <= Bottom;
        }

        public bool Contains(RectF area)
        {
            if (Points.Any(x => area.Contains(x)))
                return true;

            if (area.Points.Any(X => Contains(X)))
                return true;

            if (Left < area.Left && Right > area.Right && Top < area.Top && Bottom > area.Bottom)
                return true;

            if (area.Left < Left && area.Right > Right && area.Top < Top && area.Bottom > Bottom)
                return true;

            return false;
        }
    }
}
