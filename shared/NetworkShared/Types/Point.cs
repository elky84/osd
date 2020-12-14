using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkShared.Types
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public double Delta(Point p)
        {
            return Math.Abs(X - p.X) + Math.Abs(p.Y - p.Y);
        }

        public double Delta(FlatBuffers.Protocol.Position p)
        {
            return Delta(new Point { X = p.X, Y = p.Y });
        }
    }
}
