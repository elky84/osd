using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using Newtonsoft.Json;

// TODO : 이동 불가 구역 체크기능 필요.
public enum MAP_TYPE
{
    Move = 0,
    Block = 1
}

[ExecuteInEditMode]
[Serializable]
public class MapTool : MonoBehaviour
{
    public string StageFileName;
    public string TileSetName;
    public Vector2Int MapTileSize;
    public int[] BackGroundIndex;      // 배경 이미지 인덱스
    public int[] MapIndex;      // 타일 이미지 인덱스 (실제로 보여질)
    public int[] MapType;       // 이동가능 , 이동불가능  ( 체크박스로 block 추가 / 표시)

    public void MakeNewMap()
    {
        var tileMapObject = GameObject.Find("TileMap");

        var mapObject = GameObject.Find("TileMap/Map");
        UnityEngine.Tilemaps.Tilemap tileMap = mapObject.GetComponent<UnityEngine.Tilemaps.Tilemap>();
        tileMap.ClearAllTiles();

        var blockObject = GameObject.Find("TileMap/Block");
        if (blockObject == null)
        {
            blockObject = Instantiate(mapObject, tileMapObject.transform);
            blockObject.name = "Block";
        }
        UnityEngine.Tilemaps.Tilemap block = blockObject.GetComponent<UnityEngine.Tilemaps.Tilemap>();
        block.color = new Color(1, 0.5f, 0.5f, 0.5f);
        block.ClearAllTiles();


        var backgroundMapObject = GameObject.Find("TileMap/Background");
        if (backgroundMapObject == null)
        {
            backgroundMapObject = Instantiate(mapObject, tileMapObject.transform);
            backgroundMapObject.name = "Background";
            backgroundMapObject.transform.SetAsFirstSibling();
        }
        UnityEngine.Tilemaps.Tilemap backgroundMap = backgroundMapObject.GetComponent<UnityEngine.Tilemaps.Tilemap>();
        backgroundMap.ClearAllTiles();

        BackGroundIndex = new int[MapTileSize.x * MapTileSize.y];
        MapIndex = new int[MapTileSize.x * MapTileSize.y];
        MapType = new int[MapTileSize.x * MapTileSize.y];
        Array.Clear(BackGroundIndex, 0, BackGroundIndex.Length);
        Array.Clear(MapIndex, 0, MapIndex.Length);
        Array.Clear(MapType, 0, MapType.Length);
    }

    public void LoadMap(string path)
    {
        string fullPath = path + StageFileName + ".json";

        MakeNewMap();
        if (File.Exists(fullPath))
        {
            string jsonData = File.ReadAllText(path + StageFileName + ".json");
            JsonUtility.FromJsonOverwrite(jsonData, this);
            ApplyDataToTile();
        }
    }

    public void LoadMap(MapData mapData)
    {
        MakeNewMap();
        JsonUtility.FromJsonOverwrite(JsonConvert.SerializeObject(mapData), this);
        ApplyDataToTile();
    }

    public void clear()
    {
        this.StageFileName = "";
        MapTileSize = new Vector2Int(0, 0);

        var mapObject = GameObject.Find("TileMap/Map");
        UnityEngine.Tilemaps.Tilemap tileMap = mapObject.GetComponent<UnityEngine.Tilemaps.Tilemap>();
        tileMap.ClearAllTiles();

        var blockObject = GameObject.Find("TileMap/Block");
        UnityEngine.Tilemaps.Tilemap block = blockObject.GetComponent<UnityEngine.Tilemaps.Tilemap>();
        block.ClearAllTiles();

        var backgroundMapObject = GameObject.Find("TileMap/Background");
        UnityEngine.Tilemaps.Tilemap backgroundMap = backgroundMapObject.GetComponent<UnityEngine.Tilemaps.Tilemap>();
        backgroundMap.ClearAllTiles();

        BackGroundIndex = null;
        MapIndex = null;
        MapType = null;
    }
    public void ApplyDataToTile()
    {
        var backgroundMapObject = GameObject.Find("TileMap/Background");
        UnityEngine.Tilemaps.Tilemap backgroundMap = backgroundMapObject.GetComponent<UnityEngine.Tilemaps.Tilemap>();

        var mapObject = GameObject.Find("TileMap/Map");
        UnityEngine.Tilemaps.Tilemap tileMap = mapObject.GetComponent<UnityEngine.Tilemaps.Tilemap>();

        var blockObject = GameObject.Find("TileMap/Block");
        UnityEngine.Tilemaps.Tilemap block = blockObject.GetComponent<UnityEngine.Tilemaps.Tilemap>();

        BoundsInt bounds = new BoundsInt(Vector3Int.zero, new Vector3Int(MapTileSize.x, MapTileSize.y, 0));
        var sprites = Resources.LoadAll("Tilesets/" + TileSetName);
        var tiles = new UnityEngine.Tilemaps.TileBase[MapTileSize.x * MapTileSize.y];
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                //  Debug.Log("x:" + x + " y:" + y + " tile:" + MapIndex[x + (y * MapTileSize.x)]);

                var tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
                if (MapIndex[x + (y * MapTileSize.x)] > 0)
                {
                    tile.sprite = sprites[MapIndex[x + (y * MapTileSize.x)]] as Sprite;
                    tile.name = sprites[MapIndex[x + (y * MapTileSize.x)]].name;
                    tileMap.SetTile(new Vector3Int(x, y, 0), tile);

                }

                var backtile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
                if (BackGroundIndex[x + (y * MapTileSize.x)] > 0)
                {
                    backtile.sprite = sprites[BackGroundIndex[x + (y * MapTileSize.x)]] as Sprite;
                    backtile.name = sprites[BackGroundIndex[x + (y * MapTileSize.x)]].name;
                    backgroundMap.SetTile(new Vector3Int(x, y, 0), backtile);

                }

                if (MapType[x + (y * MapTileSize.x)] == (int)MAP_TYPE.Block)
                    block.SetTile(new Vector3Int(x, y, 0), backtile);
            }
        }

    }
    public void ExportToJson(string path)
    {
        // MapData 데이터 옮기고 익스포트
        var backgroundMapObject = GameObject.Find("TileMap/Background");
        UnityEngine.Tilemaps.Tilemap backgroundMap = backgroundMapObject.GetComponent<UnityEngine.Tilemaps.Tilemap>();

        var mapObject = GameObject.Find("TileMap/Map");
        UnityEngine.Tilemaps.Tilemap tileMap = mapObject.GetComponent<UnityEngine.Tilemaps.Tilemap>();

        var blockObject = GameObject.Find("TileMap/Block");
        UnityEngine.Tilemaps.Tilemap block = blockObject.GetComponent<UnityEngine.Tilemaps.Tilemap>();

        BoundsInt bounds = new BoundsInt(Vector3Int.zero, new Vector3Int(MapTileSize.x, MapTileSize.y, 1));
        UnityEngine.Tilemaps.TileBase[] allTiles = tileMap.GetTilesBlock(bounds);
        UnityEngine.Tilemaps.TileBase[] blockTiles = block.GetTilesBlock(bounds);
        UnityEngine.Tilemaps.TileBase[] backTiles = backgroundMap.GetTilesBlock(bounds);

        UnityEngine.Object[] sprites = null;
        // Tile
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                UnityEngine.Tilemaps.TileBase tile = allTiles[x + y * bounds.size.x];

                if (tile != null && !tile.name.Equals(""))
                {
                    Debug.Log("x:" + x + " y:" + y + " tile:" + tile.name);
                    if (sprites == null)
                    {
                        Debug.Log(tile.name);

                        int charIndex = tile.name.LastIndexOf("_", tile.name.Length);
                        TileSetName = tile.name.Substring(0, charIndex);
                        sprites = Resources.LoadAll("Tilesets/" + TileSetName);
                    }

                    int index = Array.FindIndex(sprites, s => s.name == tile.name);
                    if (index == -1)
                        MapIndex[x + (y * MapTileSize.x)] = 0;
                    else
                        MapIndex[x + (y * MapTileSize.x)] = index;
                }
                else
                {
                    //Debug.Log("x:" + x + " y:" + y + " tile: (null)");
                    MapIndex[x + (y * MapTileSize.x)] = 0;
                }
            }
        }

        // Block
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                UnityEngine.Tilemaps.TileBase blockTile = blockTiles[x + y * bounds.size.x];

                if (blockTile == null)
                {
                    // 이동가능
                    MapType[x + (y * MapTileSize.x)] = (int)MAP_TYPE.Move;
                }
                else
                {
                    MapType[x + (y * MapTileSize.x)] = (int)MAP_TYPE.Block;
                }

            }
        }

        // BackGround
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                UnityEngine.Tilemaps.TileBase backTile = backTiles[x + y * bounds.size.x];

                if (backTile != null && !backTile.name.Equals(""))
                {
                    //Debug.Log("x:" + x + " y:" + y + " tile:" + backTile.name);

                    if (sprites == null)
                    {
                        int charIndex = backTile.name.LastIndexOf("_", backTile.name.Length);
                        TileSetName = backTile.name.Substring(0, charIndex);
                        sprites = Resources.LoadAll("Tilesets/" + TileSetName);
                    }
                    int index = Array.FindIndex(sprites, s => s.name == backTile.name);
                    if (index == -1)
                        BackGroundIndex[x + (y * MapTileSize.x)] = 0;
                    else
                        BackGroundIndex[x + (y * MapTileSize.x)] = index;
                }
                else
                {
                    //Debug.Log("x:" + x + " y:" + y + " tile: (null)");
                    BackGroundIndex[x + (y * MapTileSize.x)] = 0;
                }
            }
        }

        // export할 경로 설정
        string fileFullPath = path + StageFileName + ".json";
        SaveToFile(fileFullPath);

    }
    public void SaveToFile(string fileFullPath)
    {
        var jsonData = JsonUtility.ToJson(this);
        Debug.Log(jsonData);
        File.WriteAllText(fileFullPath, jsonData.ToString());
    }

}