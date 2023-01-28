//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Shiny_Reflect"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_MainTex2("Base (RGB)", 2D) = "white" {}
	_Color("_Color", Color) = (1,1,1,1)
		_Distortion("Distortion", Range(0,1)) = 0
		_Value2("Value 2", Range(0,1)) = 0
		_Value3("Value 3", Range(0,1)) = 0
		_Value4("Value 4", Range(0,1)) = 0
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
	sampler2D _MainTex2;
	float _Distortion;
	float _Value2;
	float _Value3;
	float _Value4;
	float _Value5;
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




		float4 tex = tex2D(_MainTex, IN.uv_MainTex)*IN.color;
		float4 tex2 = tex2D(_MainTex2, IN.uv_MainTex)*IN.color;
		float3 lum = float3(0.299, 0.587, 0.114);
		float x = dot(tex.rgb, lum).r;
		float t = tex2.r + x*_Value5;

		t = smoothstep(t,t + _Value2, _Distortion + _Value2*0.5);
		t -= 1 - t;
		t = 1 - abs(t);
		t = abs(t);
		t *= _Value3;
		tex.rgb += float3(t,t,t);
		tex.a = tex.a*(1.0 - _Alpha);
		tex2 = float4(1,1,1,t*tex.a);
		tex = lerp(tex,tex2,_Value4);

		o.Albedo = tex.rgb * tex.a;
		o.Alpha = tex.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}