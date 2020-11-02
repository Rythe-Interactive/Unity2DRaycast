using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
public struct RayTracingObject
{
    float2 pos;
    float depth;
    float radius;
    int ObjectType; //0=sphere, 1 = quad

    float3 albedo;
    float3 specular;
    float smoothness;
    float3 emission;
    int textureIndex;

    public RayTracingObject(float3 position, float r, int newobjectType, float3 newAlbedo, float3 newSpec, float smooth, float3 e, int texInd)
    {
        pos = new float2(position.x, position.y);
        depth = position.z;
        radius = r;
        ObjectType = newobjectType;
        albedo = newAlbedo;
        specular = newSpec;
        smoothness = smooth;
        emission = e;
        textureIndex = texInd;
    }
}
public static class Converter
{
    public static RayTracingObject ConvertSphere(Sphere sphere)
    {
        RayTracingObject r = new RayTracingObject(sphere.position, sphere.radius, 0, sphere.albedo, sphere.specular, sphere.smoothness, sphere.emission, 0);
        return r;
    }
    public static RayTracingObject ConvertSprite(SpriteRT sprite)
    {
        int r = sprite.TextureDimensions;
        float2 center = new float2(sprite.posMin) + new float2(r, r);
        RayTracingObject rt = new RayTracingObject(new float3(center.x, center.y, sprite.depth), r, 1, float3.zero, new float3(0.3f), 0.5f, new float3(0.0f), sprite.TextureIndex);
        return rt;

    }

}
