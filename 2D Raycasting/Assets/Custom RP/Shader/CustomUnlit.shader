Shader "Unlit/CustomUnlit"
{

	Properties
	{
	_BaseMap("Texture", 2D) = "White"{}
	_BaseColor("Color", color) = (1.0, 1.0, 1.0, 1.0)
	_Cutoff("Alpha Cutoff" , Range(0.0,1.0)) = 0.5
	[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", float) = 1
	[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", float) = 0
	[Enum(Off,0, On,1)] _ZWrite("Z write", float) = 1
	}


	SubShader
	{
		Pass
		{
		ZWrite[_ZWrite]
		Blend[_SrcBlend][_DstBlend]

		HLSLPROGRAM
#pragma target 3.5
#pragma multi_compile_instancing

#pragma vertex UnlitPassVertex
#pragma fragment UnlitPassFragment

#include "UnlitPass.hlsl"
		ENDHLSL
		}
	}
}
