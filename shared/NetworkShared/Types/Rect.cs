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
    }
}
