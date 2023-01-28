//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Lightning"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	[HideInInspector] _MainTex2("Pattern (RGB)", 2D) = "white" {}
	[HideInInspector] _Alpha("Alpha", Range(0,1)) = 1.0
		[HideInInspector] _Color("Tint", Color) = (1,1,1,1)
		[HideInInspector] _Value1("_Value1", Range(0,1)) = 0
		[HideInInspector] _Value2("_Value2", Range(0,1)) = 0
		[HideInInspector] _Value3("_Value3", Range(0,1)) = 0
		[HideInInspector] _Value4("_Value4", Range(0,1)) = 0
		[HideInInspector] _Value5("_Value5", Range(0,1)) = 0
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
	sampler2D _MainTex2;
	float4 _Color;
	float _Alpha;
	float _Value1;
	float _Value2;
	float _Value3;
	float _Value4;
	float _Value5;

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

		float speed = _Value1;
		float2 uv = IN.uv_MainTex;
		uv += float2(0,0);
		uv /= 8;
		float tm = _Time;
		uv.x += floor(fmod(tm*speed, 1.0) * 8) / 8;
		uv.y += (1 - floor(fmod(tm*speed / 8, 1.0) * 8) / 8);
		float4 t2 = tex2D(_MainTex2, uv);

		uv = IN.uv_MainTex;
		uv /= 8;
		tm += 0.2;
		uv /= 1.0;
		uv.x += floor(fmod(tm*speed, 1.0) * 8) / 8;
		uv.y += (1 - floor(fmod(tm*speed / 8, 1.0) * 8) / 8);
		t2 += tex2D(_MainTex2, uv);

		uv = IN.uv_MainTex;
		uv /= 8;
		tm += 0.6 + _Time;
		uv.x += floor(fmod(tm*speed, 1.0) * 8) / 8;
		uv.y += (1 - floor(fmod(tm*speed / 8, 1.0) * 8) / 8);
		t2 += tex2D(_MainTex2, uv);

		float4 t = tex2D(_MainTex, IN.uv_MainTex)*IN.color;
		t2.a = t.a;


		t.rgb += t2*_Value2;


		float4 c = float4(t.rgb,t.a*(1 - _Alpha));



		o.Albedo = c.rgb * c.a;
		o.Alpha = c.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}