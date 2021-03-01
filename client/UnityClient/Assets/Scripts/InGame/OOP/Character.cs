using UnityEngine;

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

namespace Assets.Scripts.InGame.OOP
{
    public class Character : Assets.Scripts.InGame.OOP.Life
    {
        public int Level { get; set; }
        public long Exp { get; set; }

        public CharacterStateType State { get; set; } = CharacterStateType.Idle;

        public override NetworkShared.ObjectType Type => NetworkShared.ObjectType.Character;

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

        public void ActiveSkill(int slot = 0)
        {
            NettyClient.Instance.Send(FlatBuffers.Protocol.Request.ActiveSkill.Bytes(slot));
        }

        public void Attack()
        {
            NettyClient.Instance.Send(FlatBuffers.Protocol.Request.Attack.Bytes());
        }
    }
}
