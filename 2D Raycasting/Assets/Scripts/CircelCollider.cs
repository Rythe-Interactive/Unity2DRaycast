using Unity.Mathematics;
using System;
using System.Collections;
using UnityEngine;
public struct CircleCollider
{

    public float2 Center;
    public float Radius;
    public float rSquared;
    public CircleCollider(float2 pos, float r)
    {
        Radius = r;
        Center = pos;
        rSquared = r * r;
    }
    public CircleCollider(AABB aabb)
    {
        Center = (aabb.Min + aabb.Max) * 0.5f;
        float dimx = aabb.Max.x - aabb.Min.x;
        float dimy = aabb.Max.y - aabb.Min.y;

        Radius = math.max(dimx, dimy) * 0.5f;
        rSquared = Radius * Radius;
    }
}
public static class PointCircleIntersection
{

    public static bool Intersection(CircleCollider circle, float2 pos)
    {
        float2 dir = circle.Center - pos;
        float dist = math.length(dir);
        return false;
    }
}
public static class RayCircleIntersection
{

    public static bool Intersection(CircleCollider circle, Ray ray)
    {

        float2 dir = ray.Origin - circle.Center;
        float b = math.dot(dir, ray.Direction);
        float c = math.dot(dir, dir) - circle.rSquared;
        if (c > 0.0f && b > 0.0f) return false;
        float discr = b * b - c;
        if (discr < 0) return false;

        return true;
    }
}
