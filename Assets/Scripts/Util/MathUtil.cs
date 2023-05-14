using System;
using UnityEngine;

public static class MathUtil
{
    // In C#, % operator gets the remainder, not modulus, so will not work properly with negative numbers
    public static int Mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }

    public static Vector3 GetRandomPointWithAngleDistance(Vector3 origin, float distMin, float distMax)
    {
        float angle = UnityEngine.Random.Range(0f, 360f);
        float distance = UnityEngine.Random.Range(distMin, distMax);

        float x = distance * Mathf.Sin(angle);
        float z = distance * Mathf.Cos(angle);

        return new Vector3(origin.x + x, 0, origin.z + z);
    }

    public static Vector3 Abs(this Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    public static Vector2Int Abs(this Vector2Int v)
    {
        return new Vector2Int(Mathf.Abs(v.x), Mathf.Abs(v.y));
    }

    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);

        return v;
    }

    public static Vector2Int ToVector2Int(this Vector2 v)
    {
        return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
    }

    public static T RandomElementOfArray<T>(T[] array) 
    {
        if(array.Length == 0)
        {
            return default;
        }

        int index = UnityEngine.Random.Range(0, array.Length);
        return array[index];
    }
}