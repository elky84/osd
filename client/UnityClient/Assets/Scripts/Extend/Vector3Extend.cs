using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Vector3Extend
{
    public static Vector3 Change(this Vector3 vector3, float x, float y, float z)
    {
        return new Vector3(vector3.x + x, vector3.y + y, vector3.z + z);
    }

    public static Vector2 ToVector2(this Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.y);
    }
}