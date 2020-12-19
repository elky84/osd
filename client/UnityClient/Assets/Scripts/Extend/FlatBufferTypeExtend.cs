using FlatBuffers.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class FlatBufferTypeExtend
{
    public static Vector3 ToVector3(this Position position)
    {
        return new Vector3((float)position.X, (float)position.Y);
    }

    public static Vector2 ToVector2(this Position position)
    {
        return new Vector3((float)position.X, (float)position.Y);
    }

    public static Vector3 ToVector3(this Position.Model position)
    {
        return new Vector3((float)position.X, (float)position.Y);
    }

    public static Vector2 ToVector2(this Position.Model position)
    {
        return new Vector3((float)position.X, (float)position.Y);
    }
}