using KeraLua;
using System;
using System.Drawing;

namespace TestServer.Model
{
    public class Position
    {
        public double X { get; set; }

        public double Y { get; set; }

        public Position()
        {
            X = 0;
            Y = 0;
        }

        public Position(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public Point ToPoint()
        {
            return new Point((int)X, (int)Y);
        }

        public double Delta(Position p)
        {
            return Math.Abs(X - p.X) + Math.Abs(p.Y - p.Y);
        }
    }

}
