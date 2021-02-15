using NetworkShared;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Position
{
    public float X { get; set; }

    public float Y { get; set; }

    public Position()
    {

    }

    public Position(float x, float y)
    {
        this.X = x;
        this.Y = y;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(X, Y);
    }

    public static Position FromFlatBuffer(FlatBuffers.Protocol.Response.Vector2 position)
    {
        return new Position { X = (float)position.X, Y = (float)position.Y };
    }
}

public enum Direction
{
    Left, Right, Top, Bottom
}

public class Character : SpriteObject
{
    public static float JUMPING_POWER = 30.0f;
    public static float SPEED = 1.0f;


    public Text NickName;

    public float Speed => 1;
    public Vector2 Velocity { get; set; }

    private IEnumerator MoveCoroutine { get; set; }

    public CharacterStateType State { get; set; } = CharacterStateType.Idle;

    public Position CurrentPosition { get; set; } = new Position();

    public Direction TargetDirection;

    public bool Moving { get; private set; }

    public DateTime MoveDt { get; set; }

    public ObjectType Type { get; set; }

    public int Sequence { get; set; }

    public string Name { get; set; }

    public Action<Character, Collision2D> OnCollisionEnter { get; set; }
    public Action<Character, Collision2D> OnCollisionExit { get; set; }
    public Action<Character> OnJump { get; set; }


    public void Awake()
    {
        OnAwake();
    }

    public void Start()
    {
        NickName.text = Name;
    }

    //바닥에 붙어있는지 판별
    //public으로 되어 있는 이유는 실핼중에 bool값을 체크하기 위해
    public bool IsGround;

    //땅에 접촉한 동안에 실행됨
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.tag == "Tile")
        {
            this.IsGround = true;
        }
    }

    //땅에서 탈출한 시점에 실행됨
    private void OnCollisionExit2D(Collision2D collision)
    {
        var before = this.IsGround;
        if (collision.transform.tag == "Tile")
        {
            this.IsGround = false;
        }

        if(before && !this.IsGround)
            this.OnCollisionExit?.Invoke(this, collision);
    }

    public void Attacking()
    {
    }

    public void DamageEnd()
    {
    }

    public void DeadEnd()
    {
    }

    private void MoveStart()
    {
        MoveCoroutine = CoMove();
        StartCoroutine(MoveCoroutine);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        this.OnCollisionEnter?.Invoke(this, collision);
    }

    private IEnumerator CoMove()
    {
        Animator.SetBool("Walking", true);
        while (true)
        {
            var end = DateTime.Now;
            var diff = end - MoveDt;
            MoveDt = end;

            var moved = (Velocity * diff.Ticks) / 1000000;
            CurrentPosition = new Position(CurrentPosition.X + moved.x, transform.localPosition.y);
            transform.localPosition = CurrentPosition.ToVector3();
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void RemoveRigidBody()
    {
        var collider2D = this.gameObject.GetComponent<BoxCollider2D>();
        if (collider2D != null)
        {
            collider2D.enabled = false;
        }
    }

    public void MoveDirection(Direction direction, bool packetSend)
    {
        if (Moving && this.TargetDirection == direction)
            return;

        this.TargetDirection = direction;
        StopMove(packetSend);

        this.Moving = true;
        this.Velocity = new Vector2(1.0f * (direction == Direction.Left ? -SPEED : SPEED), 0);
        MoveDt = DateTime.Now;

        if (packetSend)
        {
            NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Move.Bytes(this.Sequence, new FlatBuffers.Protocol.Request.Vector2.Model { X = this.transform.localPosition.x, Y = this.transform.localPosition.y }, (int)direction));
        }

        MoveStart();
    }

    public void Jump()
    {
        if (IsGround)
        {
            var rigidBody2D = this.gameObject.GetComponent<Rigidbody2D>();
            rigidBody2D.AddForce(Vector3.up * JUMPING_POWER);

            OnJump?.Invoke(this);
        }
    }

    public void StopMove(bool packetSend)
    {
        try
        {
            this.Moving = false;
            this.Velocity = new Vector2(0, 0);
            if (MoveCoroutine != null)
            {
                StopCoroutine(MoveCoroutine);
                MoveCoroutine = null;
                Animator.SetBool("Walking", false);

                if (packetSend)
                    NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Stop.Bytes(this.Sequence, new FlatBuffers.Protocol.Request.Vector2.Model { X = this.transform.localPosition.x, Y = this.transform.localPosition.y }));
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }


    public void KeyUp(Direction direction)
    {
        if (direction == TargetDirection)
        {
            StopMove(true);
        }
    }
}
