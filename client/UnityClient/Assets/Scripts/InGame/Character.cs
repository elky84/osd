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

    public static Position FromFlatBuffer(FlatBuffers.Protocol.Position position)
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

    private IEnumerator MoveCoroutine { get; set; }

    public CharacterStateType State { get; set; } = CharacterStateType.Idle;

    public Position CurrentPosition { get; set; } = new Position();

    public Direction TargetDirection;

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

            //Debug.Log($"Current: {CurrentPosition.X} {CurrentPosition.Y} Diff:{diff.TotalMilliseconds}");

            float moved = (float)(diff.TotalMilliseconds * (Speed / 1000.0));
            switch (TargetDirection)
            {
                case Direction.Left:
                    CurrentPosition = new Position(CurrentPosition.X - moved, CurrentPosition.Y);
                    FlipX = true;
                    break;
                case Direction.Top:
                    CurrentPosition = new Position(CurrentPosition.X, CurrentPosition.Y + moved);
                    break;
                case Direction.Right:
                    CurrentPosition = new Position(CurrentPosition.X + moved, CurrentPosition.Y);
                    FlipX = false;
                    break;
                case Direction.Bottom:
                    CurrentPosition = new Position(CurrentPosition.X, CurrentPosition.Y - moved);
                    break;
                default:
                    throw new Exception("Invalid direction value");
            }

            //Debug.Log($"Move: {CurrentPosition.X} {CurrentPosition.Y}");

            transform.localPosition = CurrentPosition.ToVector3();
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void MoveDirection(Direction direction, bool packetSend)
    {
        if (this.TargetDirection == direction)
        {
            return;
        }

        this.TargetDirection = direction;
        StopMove(packetSend);
        MoveDt = DateTime.Now;

        if (packetSend)
        {
            //NettyClient.Instance.Send(Move.Bytes(new FlatBuffers.Protocol.Position.Model(CurrentPosition.X, CurrentPosition.Y), MoveDt.Ticks, (int)direction));
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
            Animator.SetBool("Walking", false);

            if (packetSend)
            {
                var end = DateTime.Now;
                NettyClient.Instance.Send(Stop.Bytes(new FlatBuffers.Protocol.Position.Model(CurrentPosition.X, CurrentPosition.Y), end.Ticks));
            }
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
