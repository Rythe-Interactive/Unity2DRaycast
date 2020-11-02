Shader "Unlit/CustomUnlit"
{
	Properties
	{
		_MainTex("_MainTex", 2D) = "white" {}
		_NormalMap("Normal",2D) = "blue"{}
	}

		SubShader
		{

			//standard pass
			Pass
			{
			ZWrite On
			HLSLPROGRAM
	#pragma vertex UnlitPassVertex
	#pragma fragment UnlitPassFragment
	#include "UnlitPass.hlsl"
			ENDHLSL
			}

			Pass
			{
			Tags{"LightMode" = "DepthOnly"}
			ZWrite On

			HLSLPROGRAM
	#pragma target 3.5

	#pragma vertex DepthPassVertex
	#pragma fragment DepthPassFragment
	#include "depth.hlsl"
			ENDHLSL
			}

			Pass
			{
			Tags{"LightMode" = "Voxelize"}
			ZWrite On

			HLSLPROGRAM
	#pragma target 3.5

	#pragma vertex VoxelizePassVertex
	#pragma fragment VoxelizePassFragment
	#include "VoxelizePass.hlsl"
			ENDHLSL
			}


			Pass
			{
			Tags{"LightMode" = "NormalPass"}
			ZWrite On

			HLSLPROGRAM
	#pragma target 3.5

	#pragma vertex NormalPassVertex
	#pragma fragment NormalPassFragment

	#include "NormalPass.hlsl"
			ENDHLSL
			}


			Pass
			{
			Tags{"LightMode" = "PositionPass"}
			ZWrite On

			HLSLPROGRAM
	#pragma target 3.5

	#pragma vertex PositionPassVertex
	#pragma fragment PositionPassFragment
	#include "WorldPosPass.hlsl"
			ENDHLSL
			}
		}

}

