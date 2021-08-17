Shader "Water2D"
{
	Properties
	{
		_ColorOpacity("Water Tint (RGB) & Opacity (A)", 2D) = "white" {}
		_DistortionNormalMap("Normal Map", 2D) = "bump" {}
		_DistortionStrength("Distortion strength", Float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent" }
		LOD 100

		// Grab the screen behind the object into _BackgroundTexture
		GrabPass
		{
			"_BackgroundTexture"
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
				float2 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float4 grabPos : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			sampler2D _BackgroundTexture;
			sampler2D _ColorOpacity;
			sampler2D _DistortionNormalMap;

			float _DistortionStrength;
			float4 _DistortionNormalMap_ST;
			float4 _BackgroundTexture_TexelSize;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.grabPos = ComputeGrabScreenPos(o.vertex);

#if !UNITY_UV_STARTS_AT_TOP
				if(_BackgroundTexture_TexelSize.y < 0)
					o.grabPos.y = 1.0 - o.grabPos.y;
#endif
				o.uv = v.uv;
				o.uv2 = v.uv2;

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float2 properuv = _DistortionNormalMap_ST.zw + (i.uv2.xy * _DistortionNormalMap_ST.xy);

				float3 normal1 = UnpackNormal(tex2D(_DistortionNormalMap, properuv + _Time.y));
				float3 normal2 = UnpackNormal(tex2D(_DistortionNormalMap, (1.0f - properuv) + _Time.y));

				float3 norm = (normal1 + normal2) * 0.5f * _DistortionStrength;

				half4 coloropa = tex2D(_ColorOpacity, i.uv + norm.xy * 0.1f);

				half4 bgcolor = tex2Dproj(_BackgroundTexture, i.grabPos + float4(norm.x*0.5f, norm.y*0.5f, 0, 0));

				//col = lerp(col, float4(1, 1, 1, 1), step(0.98f, i.uv.y));

				bgcolor.rgb = lerp(bgcolor.rgb, coloropa.rgb, coloropa.a);
				bgcolor.rgb *= coloropa.rgb;

				return bgcolor;
				//return float4(i.uv2.x, i.uv2.y, 0, 1);
			}
			ENDCG
		}
	}
}
