#ifndef Normal_PASS_INCLUDED
#define Normal_PASS_INCLUDED
#include "../ShaderLibrary/Common.hlsl"
TEXTURE2D(_NormalMap);
SAMPLER(sampler_NormalMap);

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

VertexOutput NormalPassVertex(VertexInput input)
{
    VertexOutput output;
    //uvs
    output.baseUV = input.baseUV;
    //world space translation
    float4 positionWS = mul(unity_ObjectToWorld, float4(input.positionOS.xyz, 1));
    output.clipPos = mul(unity_MatrixVP, positionWS);
    return output;
}
float4 NormalPassFragment(VertexOutput input) : SV_TARGET
{
    float4 normal = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.baseUV);
   // normal = float4(input.baseUV, 0, 1);
    return normal;
}
#endif

