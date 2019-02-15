using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Helper class. Class with helper functions used thought out the porject
 */
public static class Helper
{
    /// <summary>
    /// Check if two lines intersects. If they do return the point where they intersect
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <param name="p4"></param>
    /// <param name="a_intersects"></param>
    /// https://setchi.hatenablog.com/entry/2017/07/12/202756
    /// <returns></returns>
    public static bool LineInsterection(Vector2 p1, Vector2 p2, Vector2 p3, Vector3 p4, out Vector2 a_intersects)
    {
        a_intersects = Vector2.zero;

        var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

        if (d == 0.0f)
        {
            return false;
        }

        var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

        if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
        {
            return false;
        }

        a_intersects.x = p1.x + u * (p2.x - p1.x);
        a_intersects.y = p1.y + u * (p2.y - p1.y);

        return true;
    }

    /// <summary>
    /// Get the percentage value of a number 
    /// maxValue = 1250, percentage = 75%
    /// = 937.5
    /// </summary>
    /// <param name="a_maxValue"></param>
    /// <param name="a_percentage"></param>
    /// <returns></returns>
    public static float GetPercentage(float a_maxValue, float a_percentage)
    {
        float v = 0.0f;
        v = a_percentage / 100;
        v *= a_maxValue;
        return v;
    }

    /// <summary>
    /// Get the percentage
    /// </summary>
    /// <param name="a_maxValue"></param>
    /// <param name="a_number"></param>
    /// <returns></returns>
    public static float GetPercentageOfNumber(float a_maxValue, float a_number)
    {
        float v = 0.0f;
        v = a_number / a_maxValue;
        v *= 100;
        return v;
    }

    /// <summary>
    /// Loop value by incerment value by 1
    /// </summary>
    /// <param name="a_v"></param>
    /// <param name="a_maxV"></param>
    /// <returns></returns>
    public static int ALoopIndex(int a_v, int a_maxV)
    {
        return ((a_v + 1) + a_maxV) % a_maxV;
    }

    /// <summary>
    /// Loop value by decerment value by 1
    /// </summary>
    /// <param name="a_v"></param>
    /// <param name="a_maxV"></param>
    /// <returns></returns>
    public static int SLoopIndex(int a_v, int a_maxV)
    {
        return ((a_v - 1) + a_maxV) % a_maxV;
    }

    /// <summary>
    /// Loop value
    /// </summary>
    /// <param name="a_v"></param>
    /// <param name="a_av"></param>
    /// <param name="a_maxV"></param>
    /// <returns></returns>
    public static int LoopIndex(int a_v, int a_av, int a_maxV)
    {
        return ((a_v + a_av) + a_maxV) % a_maxV;
    }

    /// <summary>
    /// Parse a vector2 to a vector and return (x, 0, y)
    /// </summary>
    /// <param name="a_vector"></param>
    /// <returns></returns>
    public static Vector3 ReturnVector3(this Vector2 a_vector)
    {
        return new Vector3(a_vector.x, 0, a_vector.y);
    }

    /// <summary>
    /// Parse a vector3 to a vector 2 and return (x, z)
    /// </summary>
    /// <param name="a_vector"></param>
    /// <returns></returns>
    public static Vector2 ReturnVector2(this Vector3 a_vector)
    {
        return new Vector2(a_vector.x, a_vector.z);
    }
}
