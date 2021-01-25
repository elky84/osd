using FlatBuffers.Protocol;
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
    public Text NickName;

    public float Speed => 1;
    public Vector2 Velocity { get; set; }

    private IEnumerator MoveCoroutine { get; set; }

    public CharacterStateType State { get; set; } = CharacterStateType.Idle;

    public Position CurrentPosition { get; set; } = new Position();

    public Direction TargetDirection;

    public bool Moving => (int)Velocity.x != 0;

    public DateTime MoveDt { get; set; }

    public ObjectType Type { get; set; }

    public string Name { get; set; }


    public void Awake()
    {
        OnAwake();
    }

    public void Start()
    {
        NickName.text = Name;
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

    public void MoveStart()
    {
        MoveCoroutine = CoMove();
        StartCoroutine(MoveCoroutine);
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
            CurrentPosition = new Position(CurrentPosition.X + moved.x, CurrentPosition.Y + moved.y);
            UnityEngine.Debug.Log(CurrentPosition);
            transform.localPosition = CurrentPosition.ToVector3();
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void MoveDirection(Direction direction, bool packetSend)
    {
        if (Moving && this.TargetDirection == direction)
        {
            return;
        }

        this.TargetDirection = direction;
        StopMove(packetSend);

        this.Velocity = new Vector2(10.0f * (direction == Direction.Left ? -1 : 1), 0);
        MoveDt = DateTime.Now;

        if (packetSend)
        {
            NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Move.Bytes(new FlatBuffers.Protocol.Request.Vector2.Model { X = CurrentPosition.X, Y = CurrentPosition.Y }, (int)direction));
        }

        MoveStart();
    }

    public void Jump()
    {
        var rigidBody2D = this.gameObject.GetComponent<Rigidbody2D>();
        rigidBody2D.AddForce(Vector3.up * 10);
    }

    public void StopMove(bool packetSend)
    {
        if (MoveCoroutine != null)
        {
            StopCoroutine(MoveCoroutine);
            MoveCoroutine = null;
            Velocity = new Vector2(0, 0);
            Animator.SetBool("Walking", false);

            if (packetSend)
                NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Stop.Bytes(new FlatBuffers.Protocol.Request.Vector2.Model { X = CurrentPosition.X, Y = CurrentPosition.Y }));
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
