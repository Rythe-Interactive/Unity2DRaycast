using Unity.Mathematics;

using UnityEngine;
public struct AABB
{
    public float2 Min;
    public float2 Max;
    public float2 Normal;
    public float4 Color;
    public AABB(Vector2 newMin, Vector2 newMax)
    {
        Color = float4.zero;
        Normal = float2.zero;
        Min = newMin;
        Max = newMax;
    }
    public AABB(float2 newMin, float2 newMax)
    {
        Color = float4.zero;
        Normal = float2.zero;
        Min = newMin;
        Max = newMax;
    }
    public AABB(float2 newMin, float2 newMax, float2 normal)
    {
        Color = float4.zero;
        Normal = normal;
        Min = newMin;
        Max = newMax;
    }
    public AABB(float2 newMin, float2 newMax, float4 color)
    {
        Color = color;
        Normal = float2.zero;
        Min = newMin;
        Max = newMax;
    }
    public override string ToString()
    {
        string a = Min.ToString();
        string b = Max.ToString();
        return (a + ", " + b);
    }
}
public static class AABBExtension
{
    public static bool OverlapsAABB(AABB a, AABB b)
    {
        if (a.Min.x >= b.Max.x) return false;
        if (a.Max.x <= b.Min.x) return false;
        if (a.Min.y >= b.Max.y) return false;
        if (a.Max.y <= b.Min.y) return false;
        return true;
    }
    //check if min x/ y value are smaller or max x/y value are larger, if any is true, it does not fit inside
    public static bool FitsInsideThisAABB(AABB thisAABB, AABB otherAABB)
    {
        return
            !(otherAABB.Min.x < thisAABB.Min.x || otherAABB.Min.y < thisAABB.Min.y
            || otherAABB.Max.x > thisAABB.Max.x || otherAABB.Max.y > thisAABB.Max.y);
    }
    public static void DebugDrawAABB(AABB currentAABB, float t = 100.0f)
    {
        Vector2 min = currentAABB.Min;
        Vector2 max = currentAABB.Max;
        Vector2 TopLeft = new Vector2(min.x, max.y);
        Vector2 botRight = new Vector2(max.x, min.y);
        Debug.DrawLine(min, TopLeft, RayConfig.AABB_COLOR, t);
        Debug.DrawLine(min, botRight, RayConfig.AABB_COLOR, t);
        Debug.DrawLine(TopLeft, max, RayConfig.AABB_COLOR, t);
        Debug.DrawLine(botRight, max, RayConfig.AABB_COLOR, t);
    }

    public static void DebugDrawAABB(AABB currentAABB, Color c, float t = 100.0f)
    {
        Vector2 min = currentAABB.Min;
        Vector2 max = currentAABB.Max;
        Vector2 TopLeft = new Vector2(min.x, max.y);
        Vector2 botRight = new Vector2(max.x, min.y);
        Debug.DrawLine(min, TopLeft, c, t);
        Debug.DrawLine(min, botRight, c, t);
        Debug.DrawLine(TopLeft, max, c, t);
        Debug.DrawLine(botRight, max, c, t);
    }
    public static float2 findNormal(AABB thisAABB, float2 poi)
    {
        float2 normal = new float2(0, 0);

        //check on which side of the aabb the poi is 
        if (poi.x < thisAABB.Max.x && poi.x > thisAABB.Min.x)
        {
            //poi is on upper or lower edge
            if (poi.y == thisAABB.Min.y)
                //poi is on lower edge
                normal = new float2(0, -1);
            else
                //poi is on upper edge
                normal = new float2(0, -1);
        }
        else
        {
            //ray is on left or right edge 
            if (poi.x == thisAABB.Min.x)
                //ray is on left edge
                normal = new float2(-1, 0);
            else
                //ray is on right edge
                normal = new float2(1, 0);
        }
        return normal;
    }
}