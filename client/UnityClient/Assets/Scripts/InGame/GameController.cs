using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
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
        NettyClient.Instance.OnConnected += () =>
        {
            MainThreadDispatcher.Instance.Enqueue(() =>
            {
                Createcharacter();
                var textAsset = Resources.Load("MapFile/Town") as TextAsset;
                TileMap.GenerateMap(textAsset.text);
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

    public void Createcharacter()
    {
        var gameObj = Instantiate(Resources.Load("Prefabs/Character") as GameObject, new Vector3(0, 0), Quaternion.identity, CharactersTransform);
        var character = gameObj.GetComponent<Character>();
        character.SpriteSheetPath = "ArmoredKnight";
        cineCamera.Follow = character.transform;
        MyCharacter = character;
    }

    public void Clear()
    {
        foreach (Transform child in CharactersTransform)
        {
            GameObject.Destroy(child.gameObject);
        }

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
                MyCharacter.MoveDirection(Direction.Top);
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                MyCharacter.MoveDirection(Direction.Bottom);
            }
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                MyCharacter.MoveDirection(Direction.Left);
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                MyCharacter.MoveDirection(Direction.Right);
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
