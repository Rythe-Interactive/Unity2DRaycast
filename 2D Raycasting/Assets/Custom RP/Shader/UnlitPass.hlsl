#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED
#include "../ShaderLibrary/Common.hlsl"
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

struct VertexInput
{
    float4 positionOS : POSITION;
    float2 baseUV : TEXCOORD0;
};

struct VertexOutput
{
    float4 clipPos : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
};

VertexOutput UnlitPassVertex(VertexInput input)
{
    VertexOutput output;
    //uvs
    output.baseUV = input.baseUV;
    //world space translation
    float4 positionWS = mul(unity_ObjectToWorld, float4(input.positionOS.xyz, 1));
    output.clipPos = mul(unity_MatrixVP, positionWS);
    return output;
}
float4 UnlitPassFragment(VertexOutput input) : SV_TARGET
{
    float4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.baseUV);
    return baseMap;
}
#endif

