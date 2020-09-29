using Unity.Mathematics;
using UnityEngine;

public static class RayDrawer
{
    private const float offset = 0.0075f;
    public static void VisualizePoint(float2 point)
    {

        Color c = new Color(0.25f, 1, 0.25f, 1);
        Debug.DrawLine(
            (point - new float2(offset, offset)).ConvertFloat2ToVec2()
            , (point - new float2(offset, -offset)).ConvertFloat2ToVec2(),
            c);

        Debug.DrawLine(
          (point - new float2(offset, -offset)).ConvertFloat2ToVec2()
          , (point + new float2(offset, offset)).ConvertFloat2ToVec2(),
          c);

        Debug.DrawLine(
          (point + new float2(offset, offset)).ConvertFloat2ToVec2()
          , (point + new float2(offset, -offset)).ConvertFloat2ToVec2(),
          c);

        Debug.DrawLine(
          (point + new float2(offset, -offset)).ConvertFloat2ToVec2()
          , (point - new float2(offset, offset)).ConvertFloat2ToVec2(),
          c);
    }
    public static void VisualizePoint(float2 point, Color c)
    {
        Debug.DrawLine(
            (point - new float2(offset, offset)).ConvertFloat2ToVec2()
            , (point - new float2(offset, -offset)).ConvertFloat2ToVec2(),
            c);

        Debug.DrawLine(
          (point - new float2(offset, -offset)).ConvertFloat2ToVec2()
          , (point + new float2(offset, offset)).ConvertFloat2ToVec2(),
          c);

        Debug.DrawLine(
          (point + new float2(offset, offset)).ConvertFloat2ToVec2()
          , (point + new float2(offset, -offset)).ConvertFloat2ToVec2(),
          c);

        Debug.DrawLine(
          (point + new float2(offset, -offset)).ConvertFloat2ToVec2()
          , (point - new float2(offset, offset)).ConvertFloat2ToVec2(),
          c);
    }
}
