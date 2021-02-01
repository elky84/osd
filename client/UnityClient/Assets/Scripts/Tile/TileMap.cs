using UnityEngine;
using System.IO;
using System;
using Newtonsoft.Json;

public class TileMap : MonoBehaviour
{
    public TileData.MapData MapData { get; set; }

    public void LoadMap(TileData.MapData mapData)
    {
        MapData = mapData;

        BoundsInt bounds = new BoundsInt(Vector3Int.zero, new Vector3Int(mapData.Width, mapData.Height, 0));
        //var sprites = Resources.LoadAll("Tilesets/" + mapData.Name);
        var sprites = Resources.LoadAll("Tilesets/" + "Volcano");
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
}