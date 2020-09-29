using System.Collections.Generic;
using Unity.Mathematics;
public struct TextureData
{
    public readonly AABB TextureAABBBounds;
    public readonly AABB[] OutlineAABBBounds;
    public TextureData(float2 position, float2 dimensions, List<AABB> aabbs, float scale = 1)
    {
        TextureAABBBounds = new AABB(position, position + dimensions * scale);
        OutlineAABBBounds = aabbs.ToArray();
    }
}
