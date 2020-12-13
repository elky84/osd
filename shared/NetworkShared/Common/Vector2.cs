using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkShared.Common
{
    public class Vector2
    {
        public int x { get; set; }

        public int y { get; set; }

        public Vector2()
        {

        }

        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Vector2Float
    {
        public float x { get; set; }

        public float y { get; set; }

        public Vector2Float()
        {

        }

        public Vector2Float(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
