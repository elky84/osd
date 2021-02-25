using System;

namespace NetworkShared.Types
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public static implicit operator FlatBuffers.Protocol.Request.Vector2.Model(Point p) => new FlatBuffers.Protocol.Request.Vector2.Model(p.X, p.Y);
        public static implicit operator FlatBuffers.Protocol.Response.Vector2.Model(Point p) => new FlatBuffers.Protocol.Response.Vector2.Model(p.X, p.Y);

        public Point()
        { }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double DistanceSqrt(Point from)
        {
            return Math.Pow(from.X - this.X, 2) + Math.Pow(from.Y - this.Y, 2);
        }

        public double Distance(Point from)
        {
            return Math.Sqrt(DistanceSqrt(from));
        }

        public double Delta(Point p)
        {
            return Math.Abs(X - p.X) + Math.Abs(p.Y - p.Y);
        }

        public double Delta(FlatBuffers.Protocol.Request.Vector2 p)
        {
            return Delta(new Point { X = p.X, Y = p.Y });
        }

        public double Delta(FlatBuffers.Protocol.Response.Vector2 p)
        {
            return Delta(new Point { X = p.X, Y = p.Y });
        }
    }

    public static class PointExtension
    {
        public static Point ToPoint(this FlatBuffers.Protocol.Request.Vector2? vector2)
        {
            if (vector2.HasValue)
            {
                return new Point(vector2.Value.X, vector2.Value.Y);
            }
            else
            {
                return new Point();
            }
        }
    }
}
