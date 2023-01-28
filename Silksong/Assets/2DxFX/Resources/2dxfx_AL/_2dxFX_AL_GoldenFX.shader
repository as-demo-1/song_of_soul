//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/GoldenFX"
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


		sampler2D _MainTex;
	float _Distortion;
	float _Alpha;
	float4 _Color;

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

	void surf(Input IN, inout SurfaceOutput o)
	{

		float2 uv = IN.uv_MainTex;
		float2 step = 0.004;
		float4 tex = tex2D(_MainTex, uv);
		float calc = 1.5 *_Distortion;
		float3 texA = tex2D(_MainTex, uv + float2(-step.x, -step.y) * calc).rgb;
		float3 texB = tex2D(_MainTex, uv + float2(step.x, -step.y) * calc).rgb;
		float3 texC = tex2D(_MainTex, uv + float2(-step.x,  step.y) * calc).rgb;
		float3 texD = tex2D(_MainTex, uv + float2(step.x,  step.y) * calc).rgb;
		float shadeA = dot(texA, 0.333333);
		float shadeB = dot(texB, 0.333333);
		float shadeC = dot(texC, 0.333333);
		float shadeD = dot(texD, 0.333333);
		float shade = 15.0 * pow(max(abs(shadeA - shadeD), abs(shadeB - shadeC)), 0.5);
		float3 col = lerp(float3(0.1, 0.18, 0.3), float3(0.4, 0.3, 0.2), shade);
		tex.a = tex.a * 1 - _Alpha;
		float4 r = float4(col,tex.a);
		r *= IN.color;

		o.Albedo = r.rgb * r.a;
		o.Alpha = r.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}