#ifndef POSITION_PASS_INCLUDED
#define POSITION_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerFrame)
float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)

float4x4 unity_ObjectToWorld;
CBUFFER_END

#define UNITY_MATRIX_M unity_ObjectToWorld
float near = 0.3f;
float far = 1000.0f;
struct VertexInput
{
    uint vertexID : SV_VertexID;
    float4 pos : POSITION;
};
struct VertexOutput
{
    float4 worldPos : SV_Position;
    uint vertID : BLENDINDICES0;
};


VertexOutput PositionPassVertex(VertexInput input)
{
    VertexOutput output;
    output.vertID = input.vertexID;
    //float4(input.vertexID*0.001f, 0, 0, 1);
    //float4(input.vertexID * 0.1f, 0, 0, 1);

    float4 worldPos = float4(input.pos.xyz, 1);
    //mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0f));
    worldPos = mul(unity_MatrixVP, worldPos);
    output.worldPos = worldPos;
    
    return output;
}
float4 PositionPassFragment(VertexOutput input) : SV_TARGET
{
    //return float4((input.worldPos * 0.001f).rg, 1 - input.worldPos.b, 1.0f);
   // float d = LinearEyeDepth(input.worldPos, unity_MatrixVP);
    
    return float4(input.vertID, 0, 0, 1);
    //return float4((input.worldPos * 0.001f).rgb, 1);
}
#endif