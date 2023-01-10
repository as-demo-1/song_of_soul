//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/CompressionFX"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("Tint", Color) = (1,1,1,1)
		_Distortion("Distortion", Range(0,1)) = 0
		_Alpha("Alpha", Range(0,1)) = 1.0
		[HideInInspector]_SrcBlend("_SrcBlend", Float) = 0
		[HideInInspector]_DstBlend("_DstBlend", Float) = 0
		[HideInInspector]_BlendOp("_BlendOp",Float) = 0
		[HideInInspector]_Z("_Z", Float) = 0
	}

		SubShader
	{
		Tags
	{
		"IgnoreProjector" = "True"
		"RenderType" = "TransparentCutout"
		"PreviewType" = "Plane"
		"CanUseSpriteAtlas" = "True"
	}
		Cull Off
		Lighting Off
		ZWrite[_Z]
		BlendOp[_BlendOp]
		Blend[_SrcBlend][_DstBlend]

		CGPROGRAM
#pragma surface surf Lambert vertex:vert nofog keepalpha addshadow fullforwardshadows
#pragma target 3.0
		sampler2D _MainTex;
	float4 _Color;
	float _Distortion;
	float _Alpha;

	struct Input
	{
		float2 uv_MainTex;
		float4 color;
	};

	void vert(inout appdata_full v, out Input o)
	{
		v.vertex = UnityPixelSnap(v.vertex);
		UNITY_INITIALIZE_OUTPUT(Input, o);
		o.color = v.color * _Color;
	}

	float rng2(float2 seed)
	{
		return frac(sin(dot(seed * floor(50 + (_Time + 0.1) * 12.), float2(127.1, 311.7))) * 43758.5453123);
	}

	float rng(float seed)
	{
		return rng2(float2(seed, 1.0));
	}

	void surf(Input IN, inout SurfaceOutput o)
	{
		float2 uv = IN.uv_MainTex;
		float2 blockS = floor(uv * float2(24., 19.))*4.0;
		float2 blockL = floor(uv * float2(38., 14.))*4.0;
		float r = rng2(uv);
		float lineNoise = pow(rng2(blockS), 3.0) *_Distortion* pow(rng2(blockL), 3.0);
		float4 col1 = tex2D(_MainTex, uv + float2(lineNoise * 0.02 * rng(2.0), 0))*IN.color;
		float4 result;
		result = float4(float3(col1.x, col1.y, col1.z), 1.0);
		result.a = col1.a * 1 - _Alpha;
		o.Albedo = result.rgb * result.a;
		o.Alpha = result.a;
		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}