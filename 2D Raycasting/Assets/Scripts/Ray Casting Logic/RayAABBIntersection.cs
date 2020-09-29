using Unity.Mathematics;
using UnityEngine;
public static class RayAABBIntersection
{
    //intersection check with POI calculation
    public static bool Intersection(float2 origin, float2 direction, AABB box, out float2 q)
    {
        q = float2.zero;
        float tMin = 0;
        float tMax = float.MaxValue;

        //X axis
        //check if ray is parallel to y Axis
        for (int i = 0; i < 2; i++)
        {
            if (math.abs(direction[i]) < 0.001f)
            {
                //ray is parallel check if it goes through min max x bound of box
                if (origin[i] < box.Min[i] || origin[i] > box.Max[i]) return false;
            }
            else
            {
                float ood = 1.0f / direction[i];
                float t1 = (box.Min[i] - origin[i]) * ood;
                float t2 = (box.Max[i] - origin[i]) * ood;

                tMin = math.max(tMin, math.min(t1, t2));
                tMax = math.min(tMax, math.max(t1, t2));
                //no collision found on X slab, exit early
                if (tMin > tMax) return false;
            }
        }
        if (tMin < 0) return false;
        q = origin + direction * tMin;

        return true;
    }
    //intersection check with POI calculation, && Normal calculation
    public static bool Intersection(float2 origin, float2 direction, AABB box, out float2 q, out float2 normal)
    {
        normal = Vector2.zero;

        q = float2.zero;
        float tMin = 0;
        float tMax = float.MaxValue;

        //X axis
        //check if ray is parallel to y Axis
        for (int i = 0; i < 2; i++)
        {
            if (math.abs(direction[i]) < 0.001f)
            {
                //ray is parallel check if it goes through min max x bound of box
                if (origin[i] < box.Min[i] || origin[i] > box.Max[i]) return false;
            }
            else
            {
                float ood = 1.0f / direction[i];
                float t1 = (box.Min[i] - origin[i]) * ood;
                float t2 = (box.Max[i] - origin[i]) * ood;
                tMin = math.max(tMin, math.min(t1, t2));
                tMax = math.min(tMax, math.max(t1, t2));
                //no collision found on slab, exit early
                if (tMin > tMax) return false;
            }
        }


        if (tMin < 0) return false;

        q = origin + direction * tMin;

        return true;
    }

    //just the intersection check without POI calculation
    public static bool Intersection(float2 origin, float2 direction, AABB box)
    {
        float tMin = float.MinValue;
        float tMax = float.MaxValue;

        //X axis
        //check if ray is parallel to y Axis
        for (int i = 0; i < 2; i++)
        {
            if (math.abs(direction[i]) < 0.001f)
            {
                //ray is parallel check if it goes through min max x bound of box
                if (origin[i] < box.Min[i] || origin[i] > box.Max[i]) return false;
            }
            else
            {
                float ood = 1.0f / direction[i];
                float t1 = (box.Min[i] - origin[i]) * ood;
                float t2 = (box.Max[i] - origin[i]) * ood;

                // if (t1 < 0) return false;
                tMin = math.max(tMin, math.min(t1, t2));
                tMax = math.min(tMax, math.max(t1, t2));
                //no collision found on X slab, exit early
                if (tMin > tMax) return false;

            }

        }
        if (tMin < 0) return false;
        return true;
    }
}


