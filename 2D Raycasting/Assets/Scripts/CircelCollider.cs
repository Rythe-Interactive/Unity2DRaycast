using Unity.Mathematics;

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
