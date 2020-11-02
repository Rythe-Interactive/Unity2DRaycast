Shader "Hidden/AntiAliasing"
{


	Properties
	{
		_MainTex("_MainTex", 2D) = "red" {}
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
			#include "UnityCG.cginc"

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
				return o;
			}
			Texture2D  _MainTex;
			SamplerState sampler_MainTex;
				float _Sample;
				fixed4 frag(v2f i) : SV_Target
				{
					float2 uv = i.uv;
					float4 color = _MainTex.Sample(sampler_MainTex, uv);
					float4 returnColor = float4(color.rgb, 1.0 / (_Sample + 1.0f));
				//	returnColor = float4(color.rgb, 1);
				 return returnColor;
				}
				ENDCG
			}
	}
}
