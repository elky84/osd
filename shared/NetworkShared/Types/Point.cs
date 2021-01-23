using System;
using System.Collections.Generic;
using System.Text;

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
}
