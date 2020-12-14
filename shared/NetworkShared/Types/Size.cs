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
    }
}
