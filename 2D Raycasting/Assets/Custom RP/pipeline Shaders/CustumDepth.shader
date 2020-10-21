Shader "CustomRP/CustumDepth"
{
	Properties
	{
		_BaseMap("Texture", 2D) = "White"{}
	}
	SubShader
	{
		Pass
		{
		HLSLPROGRAM

		#pragma target 3.5

		#pragma vertex DepthOnlyVertex
		#pragma fragment DepthOnlyFragment
		#include "DepthOnlyPass.hlsl"
		ENDHLSL
		}
	}
}
