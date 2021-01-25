using System;
using UnityEngine;

public static class FlatBufferTypeExtend
{
    public static Vector3 ToVector3(this FlatBuffers.Protocol.Response.Vector2 position)
    {
        return new Vector3((float)Math.Truncate(position.X * 100) / 100, (float)Math.Truncate(position.Y * 100) / 100);
    }

    public static Vector2 ToVector2(this FlatBuffers.Protocol.Response.Vector2 position)
    {
        return new Vector2((float)Math.Truncate(position.X * 100) / 100, (float)Math.Truncate(position.Y * 100) / 100);
    }

}