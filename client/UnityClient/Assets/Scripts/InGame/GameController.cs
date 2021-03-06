using Assets.Scripts.InGame.OOP;
using NetworkShared;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class GameController : MonoBehaviour
{
    public Transform ObjectGroup;
    public Transform PortalGroup;

    private Character MyCharacter { get; set; }

    private Transform TilesTransform { get; set; }

    private Dictionary<int, Assets.Scripts.InGame.OOP.Object> Objects { get; set; } = new Dictionary<int, Assets.Scripts.InGame.OOP.Object>();

    private Dictionary<int, Assets.Scripts.InGame.OOP.Object> Controllables { get; set; } = new Dictionary<int, Assets.Scripts.InGame.OOP.Object>();

    private Camera Camera { get; set; }

    public Cinemachine.CinemachineVirtualCamera cineCamera;

    public TileMap Map { get; private set; }


    void Awake()
    {
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

                Map = gameObj.GetComponent<TileMap>();
                Map.LoadMap(testMap);
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


    public void Clear()
    {
        foreach (Transform child in ObjectGroup)
        {
            GameObject.Destroy(child.gameObject);
        }

        Objects.Clear();

        foreach (Transform child in TilesTransform)
        {
            GameObject.Destroy(child.gameObject);
        }

        StopAllCoroutines();
    }


    private Assets.Scripts.InGame.OOP.Object CreateObject(string name, int sequence, ObjectType objectType, FlatBuffers.Protocol.Response.Vector2 position)
    {
        Assets.Scripts.InGame.OOP.Object obj;

        // TODO : 오브젝트 타입별로 인스턴스 다르게 생성
        switch (objectType)
        {
            case ObjectType.Character:
                {
                    var gameObj = Instantiate(Resources.Load("Prefabs/Character") as GameObject, position.ToVector3(), Quaternion.identity, ObjectGroup);
                    obj = gameObj.GetComponent<Assets.Scripts.InGame.OOP.Character>();
                }
                break;

            case ObjectType.NPC:
                {
                    var gameObj = Instantiate(Resources.Load("Prefabs/Character") as GameObject, position.ToVector3(), Quaternion.identity, ObjectGroup);
                    obj = gameObj.GetComponent<Assets.Scripts.InGame.OOP.Character>();
                }
                break;

            case ObjectType.Item:
                {
                    var gameObj = Instantiate(Resources.Load("Prefabs/Item") as GameObject, position.ToVector3(), Quaternion.identity, ObjectGroup);
                    obj = gameObj.GetComponent<Assets.Scripts.InGame.OOP.Item>();
                }
                break;

            case ObjectType.Mob:
                {
                    var gameObj = Instantiate(Resources.Load("Prefabs/Mob") as GameObject, position.ToVector3(), Quaternion.identity, ObjectGroup);
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

        obj.Map = Map;

        if (!Objects.ContainsKey(sequence))
        {
            Objects.Add(sequence, obj);
        }

        if (obj is Mob)
            (obj as Mob).OnPositionChanging = this.OnMobPositionChanging;
        return obj;
    }

    private Vector3 DontFallPosition(Life life, Vector3 position)
    {
        if (life.Direction == Direction.Right)
        {
            if (Map.IsGround(new Vector2 { x = life.BoxCollider2D.bounds.max.x, y = life.BoxCollider2D.bounds.min.y }) == MoveResult.Normal)
                return new Vector3 { x = (float)((int)life.BoxCollider2D.bounds.max.x - life.BoxCollider2D.bounds.size.x / 2.0), y = life.transform.localPosition.y };
        }
        else
        {
            if (Map.IsGround(new Vector2 { x = life.BoxCollider2D.bounds.min.x, y = life.BoxCollider2D.bounds.min.y }) == MoveResult.Normal)
                return new Vector3 { x = (float)((int)life.BoxCollider2D.bounds.min.x + life.BoxCollider2D.bounds.size.x / 2.0), y = life.transform.localPosition.y };
        }

        return position;
    }

    private Vector3 OnMobPositionChanging(Life mob, Vector3 newPosition)
    {
        return DontFallPosition(mob, newPosition);
    }

    private void RemoveObject(int sequence)
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

        obj.OnJumpEnd = this.OnJumpEnd;
        obj.OnJumpStart = this.OnJumpStart;

        if (obj.IsGround == false)
        {
            obj.OnJumpStart(obj);
            Debug.Log($"{obj.Sequence} is falling.", obj.gameObject);
        }
        else
        {
            Debug.Log($"{obj.Sequence} is on the ground.", obj.gameObject);
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
        obj.OnJumpStart = null;
        obj.OnJumpEnd = null;

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

    private void OnJumpEnd(Assets.Scripts.InGame.OOP.Object me)
    {
        // 낙하될 때 서버에 알림
        NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Fall.Bytes(me.Sequence, new FlatBuffers.Protocol.Request.Vector2.Model { X = me.transform.localPosition.x, Y = me.transform.localPosition.y }));
    }

    private void OnJumpStart(Assets.Scripts.InGame.OOP.Object me)
    {
        // 착지인지, 측면 충돌인지
        //var normal = obj.contacts[0].normal;
        Axis axis = Axis.Y;
        //if (normal.x > normal.y) // axis : x
        //    axis = Axis.X;
        //else
        //    axis = Axis.Y;

        NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Collision.Bytes(me.Sequence, new FlatBuffers.Protocol.Request.Vector2.Model { X = me.transform.localPosition.x, Y = me.transform.localPosition.y }, (int)axis));
        UnityEngine.Debug.Log($"{me.Sequence} is enter collision");
    }

    public void OnGUI()
    {
        if (EventSystem.current.IsPointerOverGameObject()) // 마우스 포인터가 UI위에 있다면
        {
            return;
        }

        if (MyCharacter == null)
        {
            return;
        }

        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                MyCharacter.Warp();
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

            else if (Input.GetKeyUp(KeyCode.Q))
            {
                MyCharacter.ActiveSkill(0);
            }
            else if (Input.GetKeyUp(KeyCode.H))
            {
                MyCharacter.ActiveSkill(1);
            }

            else if (Input.GetKeyUp(KeyCode.Z))
            {
                MyCharacter.Attack();
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
