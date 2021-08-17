// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Sprite/Shield"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[HDR]_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0

		_HitPosition("Hit Position", Vector) = (0,0,0,0)
		_HitIntensity("Hit Intensity", Float) = 0.0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex SpriteVertShield
			#pragma fragment SpriteFragSpec
			#pragma target 2.0
			#pragma multi_compile_instancing
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			#include "UnitySprites.cginc"

			struct v2fshield
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 localPos : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2fshield SpriteVertShield(appdata_t IN)
			{
				v2fshield OUT;

				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				OUT.localPos.xy = IN.vertex.xy;

	#ifdef UNITY_INSTANCING_ENABLED
				IN.vertex.xy *= _Flip;
	#endif

				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color * _RendererColor;

	#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
	#endif

				return OUT;
			}

			float3 _HitPosition;
			float _HitIntensity;

			fixed4 SpriteFragSpec(v2fshield IN) : SV_Target
			{
				float distPos = clamp(1.0f - length(IN.localPos.xy - _HitPosition.xy) / 0.5f, 0.0f, 1.0f);

				distPos = sin(distPos * (1.0f - _HitIntensity) * 5.0f * 3.14f);
				distPos = (distPos + 1) * 0.5f * _HitIntensity;

				fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
				c.rgb *= c.a + distPos;
				return c;
			}
			ENDCG
		}
	}
}
