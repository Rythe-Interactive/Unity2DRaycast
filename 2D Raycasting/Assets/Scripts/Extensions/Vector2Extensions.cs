using Unity.Mathematics;
using UnityEngine;
public static class Vector2Extensions
{
    public static float2 ConvertToFloat2(this Vector2 vec2)
    {

        return new float2(vec2.x, vec2.y);
    }
    public static Vector2 RotateVecByAngle(this Vector2 vec2, float angle)
    {
        float2x2 rotationMat = Vec2Helper.GetRotationMatrixDegree(angle);
        float2 vecAsFloat = vec2.ConvertToFloat2();
        float2 rotatedFloat2 = rotationMat.TranslateFloat2(vecAsFloat);


        return rotatedFloat2.ConvertFloat2ToVec2();
    }
    public static Vector2 Inverse(this Vector2 vec)
    {
        return new Vector2(-vec.x, -vec.y);
    }
}
