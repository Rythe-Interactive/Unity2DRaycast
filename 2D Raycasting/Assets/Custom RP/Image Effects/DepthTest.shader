Shader "ImageEffects/DepthTest"
{
	SubShader
	{
		Pass
		{
			CGPROGRAM
			//Define passes
			#pragma vertex vert
			#pragma fragment frag
			//Unity utils
			#include "UnityCG.cginc"

			sampler2D _CameraDepthTexture;
			//vertex input
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			//vertex output
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			//vertex pass
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			//fragment pass
			fixed4 frag(v2f i) : SV_Target
			{
				float4 color = float4(1,0,0,1);
				//sample depth information
				float depth = tex2D(_CameraDepthTexture, i.uv).r;
				//output depth
				color = float4(depth, depth, depth, 1);
				return color;
			}
			ENDCG
		   }
	}
}
