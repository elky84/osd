
namespace TileData
{
    public class MapData
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public string Name { get; set; }

        public Layer[] Layers { get; set; }

        public Layer Block { get; set; }

        public Layer Object { get; set; }
    }

    public class Layer
    {
        public short[] Data { get; set; }

        public string Name { get; set; }

        public string type { get; set; }

        public Object[] Objects { get; set; }
    }

    public class Rectangle
    {
        public float X { get; set; }

        public float Width { get; set; }

        public float EndX => X + Width;

        public float Y { get; set; }

        public float Height { get; set; }

        public float EndY => Y + Height;

        public override string ToString()
        {
            return $"X:{X}, Width:{Width}, Y:{Y}, Height:{Height}";
        }

        public Rectangle Normalize(int height, float size)
        {
            Y = (height - Y - Height) / size;
            Height /= size;
            X /= size;
            Width /= size;

            return this;
        }
    }

    public class Object
    {
        public string Name { get; set; }

        public string Type { get; set; }
    }
}
