using Unity.Mathematics;
static class RayLineIntersection
{
    const int RayTravelDistance = 10000;
    public static bool Intersection(float2 lineStart, float2 lineEnd, float2 rayStart, float2 RayEnd, out float2 POI)
    {
        POI = float2.zero;
        float s1_x, s2_x, s1_y, s2_y;

        s1_x = lineEnd.x - lineStart.x;
        s2_x = RayEnd.x - rayStart.x;

        s1_y = lineEnd.y - lineStart.y;
        s2_y = RayEnd.y - rayStart.y;

        float s, t;
        s = (-s1_y * (lineStart.x - rayStart.x) + s1_x * (lineStart.y - rayStart.y)) / (-s2_x * s1_y + s1_x * s2_y);
        t = (s2_x * (lineStart.y - rayStart.y) - s2_y * (lineStart.x - rayStart.x)) / (-s2_x * s1_y + s1_x * s2_y);


        if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
        {
            POI = new float2(lineStart.x + (t * s1_x), lineStart.y + (s1_y * t));
            return true;
        }

        return false;
    }
}
