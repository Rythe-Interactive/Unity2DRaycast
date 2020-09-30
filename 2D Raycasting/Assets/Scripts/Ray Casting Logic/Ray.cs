using Unity.Mathematics;
public struct Ray
{
    public float2 Origin;
    public float2 Direction;
    public Ray(float2 newOrigin, float2 direction)
    {
        Origin = newOrigin;
        Direction = math.normalize(direction);
    }
    public Ray(Ray otherRay)
    {
        Origin = otherRay.Origin;
        Direction = otherRay.Direction;
    }
    public override string ToString()
    {
        string a = Origin.ToString();
        string b = Direction.ToString();
        return ("p" + a + " + t * " + b);
    }
}
public struct CamRay
{
    public float2 Origin;
    public float3 Direction;
    public CamRay(float2 newOrigin, float3 direction)
    {
        Origin = newOrigin;
        Direction = math.normalize(direction);
    }
    public CamRay(CamRay otherRay)
    {
        Origin = otherRay.Origin;
        Direction = otherRay.Direction;
    }
    public override string ToString()
    {
        string a = Origin.ToString();
        string b = Direction.ToString();
        return ("p" + a + " + t * " + b);
    }
}

public static class RayExtension
{
    public static Ray ReflectRay(this Ray ray, float2 poi, float2 normal)
    {
        //reflection
        float2 newDir = ray.Direction - 2.0f * math.dot(normal, ray.Direction) * normal;

        // create new Ray
        Ray newRay = new Ray(poi, newDir);
        return newRay;
    }
    public static float2 GetPoint(this Ray ray, float t)
    {
        return ray.Origin + ray.Direction * t;
    }
}