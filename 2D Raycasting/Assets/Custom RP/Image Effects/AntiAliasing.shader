// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/AntiAliasing"
{


	Properties
	{
		_MainTex("Texture", 2D) = "red" {}
		_TexHeight("TextureHeight", Float) = 512
		_TexWidth("TextureWidth", Float) = 512
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
				float _TexHeight;
				float xBlur;
				float CalcLuminance(float3 color)
				{
					return dot(color, float3(0.299f, 0.587f, 0.114f));
				}

				fixed4 frag(v2f i) : SV_Target
				{
					float2 uv = i.uv;
					float3 textureColor = float3(tex2D(_MainTex, uv).rgb);
					/*const float radius = 1;
					float count = 0;
					float2 sigmaVariance = float2(0, 0);
					for (int y = -radius; y < radius; y++)
					{
						for (int x = -radius; x < radius; x++)
						{
							float2 newUV = float2(uv.x + x, uv.y + y);
							float4 CurrentColor = tex2D(_MainTex, newUV);
							float currentSample = CalcLuminance(CurrentColor.rgb);
							float SampleSquared = currentSample * currentSample;
							sigmaVariance += float2(currentSample, SampleSquared);
							count += 1.0f;
						}
					}
					sigmaVariance /= count;
					float variance = max(0.0, sigmaVariance.y - sigmaVariance.x * sigmaVariance.x);*/
					return float4(textureColor, 1.0 / (_Sample + 1.0f));
					//	return float4 (tex2D(_MainTex, i.uv).rgb, 1.0f / (_Sample + 1.0f));
					}
					ENDCG
					}
		}
}
