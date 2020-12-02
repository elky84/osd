using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    public MapTool MapTool { get; set; }

    void Start()
    {
        MapTool = gameObject.AddComponent<MapTool>();
    }

    public void GenerateMap(MapData mapData)
    {
        MapTool.LoadMap(mapData);
    }

    public void GenerateMap(string json)
    {
        MapTool.LoadMapFromJsonText(json);
    }
}
