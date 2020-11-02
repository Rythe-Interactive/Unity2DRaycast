#ifndef VOXELIZE_INLCUDED
#define VOXELIZE_INLCUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
CBUFFER_START(UnityPerFrame)
float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)

float4x4 unity_ObjectToWorld;
CBUFFER_END

#define UNITY_MATRIX_M unity_ObjectToWorld

struct VertexInput
{
    float4 pos : POSITION;
};
struct VertexOutput
{
    float4 clipPos : SV_Position;
};

VertexOutput VoxelizePassVertex(VertexInput input)
{
    VertexOutput output;
    float4 worldPos = mul(unity_ObjectToWorld, float4(input.pos.xyz, 1.0f));
    output.clipPos = mul(unity_MatrixVP, worldPos);
    return output;
}
float4 VoxelizePassFragment(VertexOutput input) : SV_TARGET
{
    return float4(1.0f,0.0f,0.0f,1.0f);
}
#endif