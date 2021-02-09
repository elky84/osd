using NetworkShared;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class GameController : MonoBehaviour
{
    private Transform CharactersTransform { get; set; }

    private Character MyCharacter { get; set; }

    private Transform TilesTransform { get; set; }

    private Dictionary<int, Character> Characters { get; set; } = new Dictionary<int, Character>();

    private Camera Camera { get; set; }

    public Cinemachine.CinemachineVirtualCamera cineCamera;


    void Awake()
    {
        CharactersTransform = transform.Find("Characters");
        Camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        TilesTransform = transform.Find("Tiles");
    }

    public void Start()
    {
        NettyClient.Instance.Init(this);

        NettyClient.Instance.OnConnected += () =>
        {
            MainThreadDispatcher.Instance.Enqueue(() =>
            {
                var gameObj = Object.Instantiate(Resources.Load($"Prefabs/TileMap") as GameObject, TilesTransform);
                gameObj.transform.SetParent(TilesTransform);

                var testMap = JsonConvert.DeserializeObject<TileData.MapData>(Resources.Load<TextAsset>("Map/test").text);

                gameObj.GetComponent<TileMap>().LoadMap(testMap);
            });
        };
        NettyClient.Instance.OnClose += () =>
        {
            MainThreadDispatcher.Instance.Enqueue(() =>
            {
                Clear();
            });
        };

        StartCoroutine(this.CoUpdate());
    }

    private Character CreateCharacter(string name, int sequence, ObjectType objectType, FlatBuffers.Protocol.Response.Vector2 position)
    {
        var gameObj = Instantiate(Resources.Load("Prefabs/Character") as GameObject, position.ToVector3(), Quaternion.identity, CharactersTransform);
        var character = gameObj.GetComponent<Character>();

        character.CurrentPosition = Position.FromFlatBuffer(position);

        character.name = name;
        character.Name = name;

        character.SpriteSheetPath = "ArmoredKnight";
        character.Type = objectType;

        Characters.Add(sequence, character);
        return character;
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
            MyCharacter.OnCollisionEnter = this.OnCollisionEnter;
            MyCharacter.OnCollisionExit = this.OnCollisionExit;
            MyCharacter.OnJump = this.OnJump;

            MyCharacter.OnCollisionExit(MyCharacter, null);
        }
    }

    private void OnJump(Character me)
    {
        NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Jump.Bytes(new FlatBuffers.Protocol.Request.Vector2.Model { X = me.transform.localPosition.x, Y = me.transform.localPosition.y }));
    }

    private void OnCollisionExit(Character me, Collision2D obj)
    {
        // 낙하될 때 서버에 알림
        NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Fall.Bytes(new FlatBuffers.Protocol.Request.Vector2.Model { X = me.transform.localPosition.x, Y = me.transform.localPosition.y }));
    }

    private void OnCollisionEnter(Character me, Collision2D obj)
    {
        // 착지인지, 측면 충돌인지
        var normal = obj.contacts[0].normal;
        Axis axis;
        if (normal.x > normal.y) // axis : x
        { 
            if(Mathf.Abs(normal.x) > 0)
                UnityEngine.Debug.Log("Left 충돌");
            else
                UnityEngine.Debug.Log("Right 충돌");

            axis = Axis.X;
        }
        else
        {
            UnityEngine.Debug.Log("Bottom 충돌");
            axis = Axis.Y;
        }

        NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Collision.Bytes(new FlatBuffers.Protocol.Request.Vector2.Model { X = me.transform.localPosition.x, Y = me.transform.localPosition.y}, (int)axis));
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
                //MyCharacter.MoveDirection(Direction.Top, true);
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                //MyCharacter.MoveDirection(Direction.Bottom, true);
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
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                MyCharacter.Jump();
            }
        }
    }

    private IEnumerator CoUpdate()
    {
        while (true)
        {
            if (this.MyCharacter != null && this.MyCharacter.Moving)
            {
                var position = new FlatBuffers.Protocol.Request.Vector2.Model { X = this.MyCharacter.transform.localPosition.x, Y = this.MyCharacter.transform.localPosition.y };
                NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Update.Bytes(position, new List<FlatBuffers.Protocol.Request.UpdateNPC.Model>()));
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
