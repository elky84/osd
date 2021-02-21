using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkShared.Types
{
    public class Size
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsEmpty => Width == 0 || Height == 0;

        public Size()
        { }

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }

    public class SizeF
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public bool IsEmpty => Width < 0.001 || Height < 0.001;

        public SizeF()
        { }

        public SizeF(float width, float height)
        {
            Width = width;
            Height = height;
        }
    }
}
