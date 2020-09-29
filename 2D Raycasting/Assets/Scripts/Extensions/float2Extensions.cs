using UnityEngine;
namespace Unity.Mathematics
{
    public static class float2Extensions
    {
        public static Vector2 ConvertFloat2ToVec2(this float2 currentFloat2)
        {
            return new Vector2(currentFloat2.x, currentFloat2.y);
        }
    }
}
