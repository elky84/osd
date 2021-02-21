using System.Collections.Generic;

namespace NetworkShared.Common
{
    public class TileSet
    { 
        public int Columns { get; set; }
        public int FirstGid { get; set; }
        public string Image { get; set; }
        public int ImageHeight { get; set; }
        public int ImageWidth { get; set; }
        public int Margin { get; set; }
        public string Name { get; set; }
        public int Spacing { get; set; }
        public int TileCount { get; set; }
        public int TileHeight { get; set; }
        public int TileWidth { get; set; }
        public string TransparentColor { get; set; }
    }

    public class Layer
    { 
        public List<short> Data { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Opacity { get; set; }
        public string Type { get; set; }
        public bool Visible { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Export
    { 
        public string Format { get; set; }
        public string Target { get; set; }
    }

    public class EditorSettings
    { 
        public Export Export { get; set; }
    }

    public class MapData
    {
        public int CompressionLevel { get; set; }
        public int Height { get; set; }
        public bool Infinite { get; set; }
        public List<Layer> Layers { get; set; }
        public int NextLAyerId { get; set; }
        public int NextObjectId { get; set; }
        public string Orientation { get; set; }
        public string RenderOrder { get; set; }
        public string TiledVersion { get; set; }
        public int TileHeight { get; set; }
        public List<TileSet> TileSets { get; set; }
        public int TileWidth { get; set; }
        public string Type { get; set; }
        public double Version { get; set; }
        public int Width { get; set; }
    }
}
