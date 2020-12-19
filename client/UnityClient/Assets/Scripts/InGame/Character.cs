using FlatBuffers.Protocol;
using NetworkShared;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum Direction
{
    Left, Right, Top, Bottom
}

public class Character : SpriteObject
{
    public Text NickName;

    public float Speed => 10;

    private IEnumerator MoveCoroutine { get; set; }

    public CharacterStateType State { get; set; } = CharacterStateType.Idle;

    public Position.Model CurrentPosition { get; set; } = new Position.Model();

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

            Debug.Log($"Current: {CurrentPosition.X} {CurrentPosition.Y} Diff:{diff.TotalMilliseconds}");

            float moved = (float)(diff.TotalMilliseconds * (Speed / 1000.0));
            switch (TargetDirection)
            {
                case Direction.Left:
                    CurrentPosition = new Position.Model(CurrentPosition.X - moved, CurrentPosition.Y);
                    FlipX = true;
                    break;

                case Direction.Top:
                    CurrentPosition = new Position.Model(CurrentPosition.X, CurrentPosition.Y + moved);
                    break;

                case Direction.Right:
                    CurrentPosition = new Position.Model(CurrentPosition.X + moved, CurrentPosition.Y);
                    FlipX = false;
                    break;

                case Direction.Bottom:
                    CurrentPosition = new Position.Model(CurrentPosition.X, CurrentPosition.Y - moved);
                    break;

                default:
                    throw new Exception("Invalid direction value");
            }

            Debug.Log($"Move: {CurrentPosition.X} {CurrentPosition.Y}");

            transform.localPosition = CurrentPosition.ToVector3();
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void MoveDirection(Direction direction)
    {
        if (this.TargetDirection == direction)
        {
            return;
        }

        this.TargetDirection = direction;
        StopMove();

        MoveDt = DateTime.Now;
        NettyClient.Instance.Send(Move.Bytes(new FlatBuffers.Protocol.Position.Model(CurrentPosition.X, CurrentPosition.Y), MoveDt.Ticks, (int)direction));

        MoveStart();
    }

    public void StopMove()
    {
        if (MoveCoroutine != null)
        {
            StopCoroutine(MoveCoroutine);
            MoveCoroutine = null;
            Animator.SetBool("Walking", false);

            var end = DateTime.Now;
            NettyClient.Instance.Send(Stop.Bytes(new FlatBuffers.Protocol.Position.Model(CurrentPosition.X, CurrentPosition.Y), end.Ticks));
        }
    }


    public void KeyUp(Direction direction)
    {
        if (direction == TargetDirection)
        {
            StopMove();
        }
    }
}
