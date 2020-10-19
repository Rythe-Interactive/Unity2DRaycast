// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/AntiAliasing"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha
		Tags
{
	"Queue" = "Transparent"
}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/NormalBuffer.hlsl"
			sampler2D _LastCameraDepthTexture;

			#include "UnityCG.cginc"
			sampler2D _CameraDepthTexture;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				//o.uv = ComputeScreenPos(o.vertex);
				return o;
			}

			sampler2D _MainTex;
			float _Sample;
			fixed4 frag(v2f i) : SV_Target
			{
				//half4 depth =Linear01Depth(tex2D(_CameraDepthTexture, i.uv).r);
				//return depth;
				/*float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				float linearDepth = Linear01Depth(depth);
				return linearDepth;*/
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);

				//return depth.r;
				return float4 (tex2D(_MainTex, i.uv).rgb, 1.0f / (_Sample + 1.0f));
				//return tex2D(_MainTex, i.uv);

			}
			ENDCG
			}
	}
}
