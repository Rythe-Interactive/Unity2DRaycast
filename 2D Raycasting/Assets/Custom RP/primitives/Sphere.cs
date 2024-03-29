﻿using Unity.Mathematics;

[System.Serializable]

public struct Sphere
{
    public float3 position;
    public float radius;
    public float3 albedo;
    public float3 specular;
    public float smoothness;
    public float3 emission;
}