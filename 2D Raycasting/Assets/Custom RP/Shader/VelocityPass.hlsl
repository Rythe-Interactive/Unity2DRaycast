#ifndef VELOCITY_INCLUDED
#define VELOCITY_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"


CBUFFER_START(UnityPerFrame)

//motion data
float4 unity_MotionVectorsParams;
float4x4 unity_MatrixPreviousM;
float4x4 unity_MatrixPreviousMI;


float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)

float4x4 unity_ObjectToWorld;
CBUFFER_END

#define UNITY_MATRIX_M unity_ObjectToWorld
struct VertexInput
{
 //   uint vertexID : SV_VertexID;
    float4 pos : POSITION;
};
struct VertexOutput
{
    float4 clipPos : SV_Position;
    float4 prevPos :COLOR;

};

VertexOutput VelocityPassVertex(VertexInput input)
{
    VertexOutput output;
        
    float4 worldPos = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0f));
    output.clipPos = mul(unity_MatrixVP, worldPos);
    
    //assume view projection has not changed since I have not found out how to acces previos VP;
    float4 prevPos = mul(unity_MatrixPreviousM, float4(input.pos.xyz, 1.0f));
    output.prevPos = mul(unity_MatrixVP, prevPos);
    return output;
}
float4 VelocityPassFragment(VertexOutput input) : SV_TARGET
{
    float2 velocity;
    float2 a = (input.clipPos.xy / input.clipPos.w) * 0.5f + 0.5f;
    float2 b = (input.prevPos.xy / input.prevPos.w) * 0.5f + 0.5f;
    velocity = float2(a - b);
    //velocity = -velocity;
    return float4(velocity.rg, 0, 1);
}
#endif