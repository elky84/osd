using FlatBuffers.Protocol;
using NetworkShared;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class GameController : MonoBehaviour
{
    private Transform CharactersTransform { get; set; }

    private TileMap TileMap { get; set; }

    private Character MyCharacter { get; set; }

    private Dictionary<int, Character> Characters { get; set; } = new Dictionary<int, Character>();

    private Camera Camera { get; set; }

    public Cinemachine.CinemachineVirtualCamera cineCamera;


    void Awake()
    {
        CharactersTransform = transform.Find("Characters");
        Camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        TileMap = GameObject.Find("TileMap").GetComponent<TileMap>();
    }

    public void Start()
    {
        NettyClient.Instance.Init(this);

        NettyClient.Instance.OnConnected += () =>
        {
            MainThreadDispatcher.Instance.Enqueue(() =>
            {
            });
        };
        NettyClient.Instance.OnClose += () =>
        {
            MainThreadDispatcher.Instance.Enqueue(() =>
            {
                Clear();
            });
        };
    }

    private void CreateCharacter(string name, int sequence, ObjectType objectType, FlatBuffers.Protocol.Position position)
    {
        var gameObj = Instantiate(Resources.Load("Prefabs/Character") as GameObject, position.ToVector3(), Quaternion.identity, CharactersTransform);
        var character = gameObj.GetComponent<Character>();

        character.CurrentPosition = Position.FromFlatBuffer(position);

        character.name = name;
        character.Name = name;

        character.SpriteSheetPath = "ArmoredKnight";
        character.Type = objectType;

        Characters.Add(sequence, character);
    }

    private void RemoveCharacter(int sequence)
    {
        var character = GetCharacter(sequence);
        if (character != null)
        {
            Destroy(character.gameObject);
            Characters.Remove(sequence);
        }
    }

    private Character GetCharacter(int sequence)
    {
        return Characters.TryGetValue(sequence, out var character) ? character : null;
    }

    private void SetMyCharacter(int sequence)
    {
        if (Characters.TryGetValue(sequence, out var character))
        {
            cineCamera.Follow = character.transform;
            MyCharacter = character;
        }
    }

    private void LoadMap(string name)
    {
        var textAsset = Resources.Load($"MapFile/{name}") as TextAsset;
        TileMap.GenerateMap(textAsset.text);
    }

    public void Clear()
    {
        foreach (Transform child in CharactersTransform)
        {
            GameObject.Destroy(child.gameObject);
        }

        TileMap.Clear();

        Characters.Clear();
    }

    public void OnGUI()
    {
        if (EventSystem.current.IsPointerOverGameObject()) // 마우스 포인터가 UI위에 있다면
        {
            return;
        }

        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                MyCharacter.MoveDirection(Direction.Top, true);
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                MyCharacter.MoveDirection(Direction.Bottom, true);
            }
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                MyCharacter.MoveDirection(Direction.Left, true);
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                MyCharacter.MoveDirection(Direction.Right, true);
            }
        }
        else if (e.type == EventType.KeyUp)
        {
            if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
            {
                MyCharacter.KeyUp(Direction.Top);
            }
            else if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
            {
                MyCharacter.KeyUp(Direction.Bottom);
            }
            else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
            {
                MyCharacter.KeyUp(Direction.Left);
            }
            else if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
            {
                MyCharacter.KeyUp(Direction.Right);
            }
        }
    }
}
