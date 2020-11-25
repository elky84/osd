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
}
