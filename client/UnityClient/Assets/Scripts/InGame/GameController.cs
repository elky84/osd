using Assets.Scripts.InGame.OOP;
using NetworkShared;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class GameController : MonoBehaviour
{
    private Transform _objectsGroup { get; set; }

    private Character MyCharacter { get; set; }

    private Transform TilesTransform { get; set; }

    private Dictionary<int, Assets.Scripts.InGame.OOP.Object> Objects { get; set; } = new Dictionary<int, Assets.Scripts.InGame.OOP.Object>();

    private Dictionary<int, Assets.Scripts.InGame.OOP.Object> Controllables { get; set; } = new Dictionary<int, Assets.Scripts.InGame.OOP.Object>();

    private Camera Camera { get; set; }

    public Cinemachine.CinemachineVirtualCamera cineCamera;


    void Awake()
    {
        _objectsGroup = transform.Find("Characters");
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
                var gameObj = UnityEngine.Object.Instantiate(Resources.Load($"Prefabs/TileMap") as GameObject, TilesTransform);
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

    private Assets.Scripts.InGame.OOP.Object CreateObject(string name, int sequence, ObjectType objectType, FlatBuffers.Protocol.Response.Vector2 position)
    {
        Assets.Scripts.InGame.OOP.Object obj;

        // TODO : 오브젝트 타입별로 인스턴스 다르게 생성
        switch (objectType)
        {
            case ObjectType.Character:
                {
                    var gameObj = Instantiate(Resources.Load("Prefabs/Character") as GameObject, position.ToVector3(), Quaternion.identity, _objectsGroup);
                    obj = gameObj.GetComponent<Assets.Scripts.InGame.OOP.Character>();
                }
                break;

            case ObjectType.NPC:
                {
                    var gameObj = Instantiate(Resources.Load("Prefabs/Character") as GameObject, position.ToVector3(), Quaternion.identity, _objectsGroup);
                    obj = gameObj.GetComponent<Assets.Scripts.InGame.OOP.Character>();
                }
                break;

            case ObjectType.Item:
                {
                    var gameObj = Instantiate(Resources.Load("Prefabs/Character") as GameObject, position.ToVector3(), Quaternion.identity, _objectsGroup);
                    obj = gameObj.GetComponent<Assets.Scripts.InGame.OOP.Character>();
                }
                break;

            case ObjectType.Mob:
                {
                    var gameObj = Instantiate(Resources.Load("Prefabs/Mob") as GameObject, position.ToVector3(), Quaternion.identity, _objectsGroup);
                    obj = gameObj.GetComponent<Assets.Scripts.InGame.OOP.Mob>();
                }
                break;

            default:
                return null;
        }
        
        obj.transform.localPosition = Position.FromFlatBuffer(position).ToVector3();
        obj.name = $"{name}({sequence})";
        obj.Name = $"{name}({sequence})";
        obj.Sequence = sequence;

        Objects.Add(sequence, obj);
        return obj;
    }

    private void RemoveCharacter(int sequence)
    {
        var character = GetObject(sequence);
        if (character != null)
        {
            UnsetControllable(character);
            Destroy(character.gameObject);
            Objects.Remove(sequence);
        }
    }

    private Assets.Scripts.InGame.OOP.Object GetObject(int sequence)
    {
        return Objects.TryGetValue(sequence, out var obj) ? obj : null;
    }

    private void SetControllable(Assets.Scripts.InGame.OOP.Object obj)
    {
        if (Controllables.ContainsKey(obj.Sequence))
            return;

        obj.OnCollisionEnter = this.OnCollisionEnter;
        obj.OnCollisionExit = this.OnCollisionExit;

        if (obj.IsGround == false)
        {
            obj.OnCollisionExit(obj, null);
            UnityEngine.Debug.Log($"{obj.Sequence} is falling.", obj.gameObject);
        }
        else
        {
            UnityEngine.Debug.Log($"{obj.Sequence} is on the ground.", obj.gameObject);
        }

        Controllables.Add(obj.Sequence, obj);
    }

    private void SetControllable(Assets.Scripts.InGame.OOP.Life obj)
    {
        if (Controllables.ContainsKey(obj.Sequence))
            return;

        obj.OnMove = this.OnMove;
        obj.OnStop = this.OnStop;
        obj.OnJump = this.OnJump;
        SetControllable(obj as Assets.Scripts.InGame.OOP.Object);
    }

    private void OnStop(Assets.Scripts.InGame.OOP.Life life)
    {
        NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Stop.Bytes(life.Sequence, new FlatBuffers.Protocol.Request.Vector2.Model { X = life.transform.localPosition.x, Y = life.transform.localPosition.y }));
    }

    private void OnMove(Assets.Scripts.InGame.OOP.Life life)
    {
        NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Move.Bytes(life.Sequence, new FlatBuffers.Protocol.Request.Vector2.Model { X = life.transform.localPosition.x, Y = life.transform.localPosition.y }, (int)life.Direction));
    }

    private void UnsetControllable(Assets.Scripts.InGame.OOP.Object obj)
    {
        obj.OnCollisionEnter = null;
        obj.OnCollisionExit = null;
        
        Controllables.Remove(obj.Sequence);
    }

    private void UnsetControllable(Assets.Scripts.InGame.OOP.Life life)
    {
        life.OnJump = null;
        life.OnMove = null;
        life.OnStop = null;
        UnsetControllable(life as Assets.Scripts.InGame.OOP.Object);
    }

    private bool SetMyCharacter(int sequence)
    {
        if (Objects.TryGetValue(sequence, out var obj) == false)
            return false;

        if (obj.Type != ObjectType.Character)
            return false;

        cineCamera.Follow = obj.transform;
        MyCharacter = obj as Character;
        SetControllable(MyCharacter);
        return true;
    }

    private void OnJump(Life me)
    {
        NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Jump.Bytes(me.Sequence, new FlatBuffers.Protocol.Request.Vector2.Model { X = me.transform.localPosition.x, Y = me.transform.localPosition.y }));
    }

    private void OnCollisionExit(Assets.Scripts.InGame.OOP.Object me, Collision2D obj)
    {
        // 낙하될 때 서버에 알림
        NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Fall.Bytes(me.Sequence, new FlatBuffers.Protocol.Request.Vector2.Model { X = me.transform.localPosition.x, Y = me.transform.localPosition.y }));
    }

    private void OnCollisionEnter(Assets.Scripts.InGame.OOP.Object me, Collision2D obj)
    {
        // 착지인지, 측면 충돌인지
        var normal = obj.contacts[0].normal;
        Axis axis;
        if (normal.x > normal.y) // axis : x
            axis = Axis.X;
        else
            axis = Axis.Y;

        NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Collision.Bytes(me.Sequence, new FlatBuffers.Protocol.Request.Vector2.Model { X = me.transform.localPosition.x, Y = me.transform.localPosition.y}, (int)axis));
        UnityEngine.Debug.Log($"{me.Sequence} is enter collision");
    }

    public void Clear()
    {
        foreach (Transform child in _objectsGroup)
        {
            GameObject.Destroy(child.gameObject);
        }

        Objects.Clear();
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
                MyCharacter.Move(Direction.Left);
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                MyCharacter.Move(Direction.Right);
            }
        }
        else if (e.type == EventType.KeyUp)
        {
            if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
            {
                if (MyCharacter.Direction == Direction.Top)
                    MyCharacter.Stop();
            }
            else if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
            {
                if (MyCharacter.Direction == Direction.Bottom)
                    MyCharacter.Stop();
            }
            else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
            {
                if (MyCharacter.Direction == Direction.Left)
                    MyCharacter.Stop();
            }
            else if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
            {
                if (MyCharacter.Direction == Direction.Right)
                    MyCharacter.Stop();
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
            foreach (var controllable in Controllables.Values)
            {
                if (controllable.Moving == false && controllable.IsGround == true)
                    continue;

                var position = new FlatBuffers.Protocol.Request.Vector2.Model { X = controllable.transform.localPosition.x, Y = controllable.transform.localPosition.y };
                NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Update.Bytes(controllable.Sequence, position, new List<FlatBuffers.Protocol.Request.UpdateNPC.Model>()));
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
