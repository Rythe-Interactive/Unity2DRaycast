using UnityEngine;
using Unity.Mathematics;
static class Vec2Helper
{
    public static float2x2 GetRotationMatrixDegree(float angle)
    {
        float radians = angle * Mathf.PI / 180;
        float2x2 rotationMat = new float2x2
            (Mathf.Cos(radians), -Mathf.Sin(radians),
            Mathf.Sin(radians), Mathf.Cos(radians));
        return rotationMat;
    }
}