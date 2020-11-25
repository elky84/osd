using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapTool))]
[CanEditMultipleObjects]
public class MapToolEditor : Editor
{
    MapTool tool;
    SerializedProperty StageFileName;
    SerializedProperty TileSetName;
    SerializedProperty MapTileSize;
    SerializedProperty MapIndex;
    SerializedProperty MapType;

    string savePath = "Assets/Resources/MapFile/";
    bool isMake = false;
    Vector2 tileSize;

    void OnEnable()
    {

        StageFileName = serializedObject.FindProperty("StageFileName");
        TileSetName = serializedObject.FindProperty("TileSetName");
        MapTileSize = serializedObject.FindProperty("MapTileSize");
        MapIndex = serializedObject.FindProperty("MapIndex");
        MapType = serializedObject.FindProperty("MapType");
        tool = (MapTool)target;

        var tileMapObject = GameObject.Find("TileMap");
        tileSize = tileMapObject.GetComponent<Grid>().cellSize;


    }
    void OnSceneGUI()
    {
        if (isMake)
        {
            Handles.color = Color.green;
            Handles.DrawLine(new Vector3(0, 0, 0), new Vector3(tool.MapTileSize.x * tileSize.x, 0, 0));
            Handles.DrawLine(new Vector3(tool.MapTileSize.x * tileSize.x, 0, 0), new Vector3(tool.MapTileSize.x * tileSize.x, tool.MapTileSize.y * tileSize.y, 0));
            Handles.DrawLine(new Vector3((tool.MapTileSize.x * tileSize.x), tool.MapTileSize.y * tileSize.y, 0), new Vector3(0, tool.MapTileSize.y * tileSize.y, 0));
            Handles.DrawLine(new Vector3(0, tool.MapTileSize.y * tileSize.y, 0), new Vector3(0, 0, 0));
        }
    }

    public override void OnInspectorGUI()
    {

        serializedObject.Update();

        if (GUILayout.Button("clear"))
        {
            tool.clear();
        }
        EditorGUILayout.PropertyField(StageFileName);
        EditorGUILayout.PropertyField(MapTileSize);
        //isBolock = EditorGUILayout.Toggle("Is Block", isBolock);

        if (GUILayout.Button("Build Map / Load Map"))
        {

            isMake = true;
            tool.LoadMap(savePath);
        }

        if (GUILayout.Button("Export"))
        {
            tool.ExportToJson(savePath);
        }
        GUILayout.Space(10);
        GUILayout.Label("Background : 배경 타일");
        GUILayout.Label("TileMap : 오브젝트 타일 이미지");
        GUILayout.Label("Block : 이동가능, 불가능(이미지 인덱스는 저장안함)");
        GUILayout.Label("직접 하이러키의 오브젝트를 클릭 OR 팔레트의 activeTilemap을 바꿔가면서 사용하세요.");
        GUILayout.Label("(activeTileMap에 접근이 안되더군요)");
        GUILayout.Space(10);
        GUILayout.Label("MapTileSize를 먼저 지정하고 BuildMap 하세요.");
        GUILayout.Label("(동일한 이름이 있으면 불러오고, 아니면 새로 만듭니다.)");


        serializedObject.ApplyModifiedProperties();
    }

}
