using UnityEngine;

public class TileMap : MonoBehaviour
{
    private short[,,] _blocks;

    public TileData.MapData MapData { get; set; }

    public void LoadMap(TileData.MapData mapData)
    {
        MapData = mapData;

        var bounds = new BoundsInt(Vector3Int.zero, new Vector3Int(mapData.Width, mapData.Height, 0));
        var sprites = Resources.LoadAll($"Tilesets/{"Volcano"}");

        _blocks = new short[mapData.Layers.Length, mapData.Height, mapData.Width];
        for (int layer = 0; layer < mapData.Layers.Length; layer++)
        {
            for (int row = 0; row < mapData.Width; row++)
            {
                for (int col = 0; col < mapData.Height; col++)
                {
                    _blocks[layer, mapData.Height - row - 1, col] = mapData.Layers[layer].Data[row * mapData.Width + col];
                }
            }
        }

        foreach (var layer in mapData.Layers)
        {
            if (layer.Data == null)
                continue;

            if (layer.Name == mapData.Block?.Name)
                continue;

            var mapObject = Instantiate(transform.Find("Map"), transform);
            mapObject.name = layer.Name;
            mapObject.transform.SetAsFirstSibling();
            UnityEngine.Tilemaps.Tilemap tileMap = mapObject.GetComponent<UnityEngine.Tilemaps.Tilemap>();

            for (int x = 0; x < bounds.size.x; x++)
            {
                for (int y = 0; y < bounds.size.y; y++)
                {
                    var spriteIndex = layer.Data[x + (mapData.Height - y - 1) * mapData.Width];
                    if (spriteIndex > 0)
                    {
                        var tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();

                        tile.sprite = sprites[spriteIndex] as Sprite;
                        tile.name = sprites[spriteIndex].name;
                        tileMap.SetTile(new Vector3Int(x, y, 0), tile);
                    }
                }
            }
        }
    }

    public bool Blocked(Vector2 position, int layer = 0)
    {
        if (position.y < 0f || position.y >= MapData.Height ||
            position.x < 0f || position.x >= MapData.Height)
        {
            return false;
        }

        var row = (int)position.y;
        var col = (int)position.x;
        return _blocks[layer, row, col] > 0;
    }

    public Vector2 FixHeight(Assets.Scripts.InGame.OOP.Object obj, Vector2 position, int layer = 0)
    {
        var colliderHeight = ((obj.BoxCollider2D.size.y * obj.transform.localScale.y) / 2.0f);
        if (Blocked(new Vector2(position.x, position.y - colliderHeight), layer))
        {
            return new Vector2(position.x, (int)position.y + colliderHeight);
        }
        else
        {
            return position;
        }
    }
}