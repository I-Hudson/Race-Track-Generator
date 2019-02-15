using UnityEngine;
using System.Collections;
using System;

/*
 * Vector 2 class impermented with doubles 
 */
public struct DVector2
{
    //x and y values for the vector
    public double x;
    public double y;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="a_newX"></param>
    /// <param name="a_newY"></param>
    public DVector2(double a_newX, double a_newY)
    {
        x = a_newX;
        y = a_newY;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="a_newVector"></param>
    public DVector2(Vector2 a_newVector)
    {
        x = a_newVector.x;
        y = a_newVector.y;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="a_newVector"></param>
    public DVector2(DVector2 a_newVector)
    {
        x = a_newVector.x;
        y = a_newVector.y;
    }

    /// <summary>
    /// Return Vector2 (Float)
    /// </summary>
    /// <returns></returns>
    public Vector2 Vector2()
    {
        return new Vector2((float)x, (float)y);
    }

    /// <summary>
    /// Return vector 3 (Float, x, 0, y)
    /// </summary>
    /// <returns></returns>
    public Vector3 Vector3()
    {
        return new Vector3((float)x, 0, (float)y);
    }

    /// <summary>
    /// Add + operator
    /// </summary>
    /// <param name="a_v0"></param>
    /// <param name="a_v1"></param>
    /// <returns></returns>
    public static DVector2 operator+ (DVector2 a_v0, DVector2 a_v1)
    {
        return new DVector2(a_v0.x + a_v1.x, a_v0.y + a_v1.y);
    }
    /// <summary>
    /// Add - operator
    /// </summary>
    /// <param name="a_v0"></param>
    /// <param name="a_v1"></param>
    /// <returns></returns>
    public static DVector2 operator- (DVector2 a_v0, DVector2 a_v1)
    {
        return new DVector2(a_v0.x - a_v1.x, a_v0.y - a_v1.y);
    }
    /// <summary>
    /// Add * operator DV * scaler
    /// </summary>
    /// <param name="a_v0"></param>
    /// <param name="a_scaler"></param>
    /// <returns></returns>
    public static DVector2 operator* (DVector2 a_v0, double a_scaler)
    {
        return new DVector2(a_v0.x * a_scaler, a_v0.y * a_scaler);
    }
    /// <summary>
    /// Add * operator DV * scaler
    /// </summary>
    /// <param name="a_scaler"></param>
    /// <param name="a_v0"></param>
    /// <returns></returns>
    public static DVector2 operator* (double a_scaler, DVector2 a_v0)
    {
        return new DVector2(a_v0.x * a_scaler, a_v0.y * a_scaler);
    }
    /// <summary>
    /// Add * operator Scaler * DV
    /// </summary>
    /// <param name="a_v0"></param>
    /// <param name="a_scaler"></param>
    /// <returns></returns>
    public static DVector2 operator* (DVector2 a_v0, float a_scaler)
    {
        return new DVector2(a_v0.x * a_scaler, a_v0.y * a_scaler);
    }
    /// <summary>
    /// Add / operator
    /// </summary>
    /// <param name="a_v0"></param>
    /// <param name="a_v1"></param>
    /// <returns></returns>
    public static DVector2 operator/ (DVector2 a_v0, DVector2 a_v1)
    {
        return new DVector2(a_v0.x / a_v1.x, a_v0.y / a_v1.y);
    }
    /// <summary>
    /// Add / operator
    /// </summary>
    /// <param name="a_v0"></param>
    /// <param name="a_scaler"></param>
    /// <returns></returns>
    public static DVector2 operator/ (DVector2 a_v0, double a_scaler)
    {
        return new DVector2(a_v0.x / a_scaler, a_v0.y / a_scaler);
    }
    /// <summary>
    /// Add == operator
    /// </summary>
    /// <param name="a_v0"></param>
    /// <param name="a_v1"></param>
    /// <returns></returns>
    public static bool operator== (DVector2 a_v0, DVector2 a_v1)
    {
        if(a_v0.x == a_v1.x && a_v0.y == a_v1.y)
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// Add != operator
    /// </summary>
    /// <param name="a_v0"></param>
    /// <param name="a_v1"></param>
    /// <returns></returns>
    public static bool operator!= (DVector2 a_v0, DVector2 a_v1)
    {
        if (a_v0.x != a_v1.x || a_v0.y != a_v1.y)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public double SqrMagnitude()
    {
        return (x * x) + (y * y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public double Magnitude()
    {
        return Math.Sqrt(x * x + y * y);
    }

    /// <summary>
    /// Return vector as a string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return string.Format("[{0},{1}]", x, y);
    }

    /// <summary>
    /// return the dot product of two vectors
    /// </summary>
    /// <param name="dv0"></param>
    /// <param name="dv1"></param>
    /// <returns></returns>
    public static double Dot(DVector2 dv0, DVector2 dv1)
    {
        return dv0.x * dv1.x + dv0.y * dv1.y;
    }
}

/*
 * Line class. Store two points
 */
public struct DLine
{
    public DVector2 v0;
    public DVector2 v1;

    /// <summary>
    /// Find the closet point along this line from a_point
    /// </summary>
    /// <param name="a_point"></param>
    /// <returns></returns>
    public DVector2 ClosestPoint(DVector2 a_point)
    {
        DVector2 delta = v1 - v0;
        double lengthSquared = delta.SqrMagnitude();
        if(lengthSquared.Equals(0f))
        {
            return v0;
        }
        double projection = DVector2.Dot(a_point - v0, delta);
        double scale = projection / lengthSquared;
        return v0 + delta * scale;
    }
}
