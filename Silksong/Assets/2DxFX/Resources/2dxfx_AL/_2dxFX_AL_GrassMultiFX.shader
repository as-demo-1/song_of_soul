//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/GrassMultiFX"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("_Color", Color) = (1,1,1,1)
		_Distortion("Distortion", Range(0,1)) = 0
		_Wind("_Wind", Range(0.0, 10.0)) = 1
		_Wind2("_Wind2", Range(0.0, 10.0)) = 1
		_Wind3("_Wind2", Range(0.0, 10.0)) = 1
		_Wind4("_Wind2", Range(0.0, 10.0)) = 1
		_Alpha("Alpha", Range(0,1)) = 1.0
		_Speed("Speed", Range(0,1)) = 1.0
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
	float _Distortion;
	float _Wind;
	float _Wind2;
	float _Wind3;
	float _Wind4;
	float _Alpha;
	float _Speed;
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
		float2 uv2 = IN.uv_MainTex + float2(0.2,0.01);
		float2 uv3 = IN.uv_MainTex + float2(0.4,0.02);
		float2 uv4 = IN.uv_MainTex + float2(0.6,0.03);
		float time = _Time * 8 * _Speed;
		float sn = uv.x + _Wind;
		float sy = uv.y / _Distortion;
		uv.x = abs(lerp(uv.x,sn,sy));
		uv.x = fmod(uv.x,1);
		sn = uv2.x + _Wind2;
		sy = uv2.y / _Distortion;
		uv2.x = abs(lerp(uv2.x,sn,sy));
		uv2.x = fmod(uv2.x,1);
		sn = uv3.x + _Wind3;
		sy = uv3.y / _Distortion;
		uv3.x = abs(lerp(uv3.x,sn,sy));
		uv3.x = fmod(uv3.x,1);
		sn = uv4.x + _Wind4;
		sy = uv4.y / _Distortion;
		uv4.x = abs(lerp(uv4.x,sn,sy));
		uv4.x = fmod(uv4.x,1);
		float4 r1 = tex2D(_MainTex, uv)* IN.color;
		float4 r2 = tex2D(_MainTex, uv2)* IN.color;
		float4 r3 = tex2D(_MainTex, uv3)* IN.color;
		float4 r4 = tex2D(_MainTex, uv4)* IN.color;
		r1.a -= 0.01;
		r2.a -= 0.01;
		r3.a -= 0.01;
		r4.a -= 0.01;
		r1.rgb -= 0.195;
		r2.rgb -= 0.130;
		r3.rgb -= 0.065;
		r4.rgb -= 0;
		float4 mo = r1 * (1.0 - (r2.a));
		float4 bo = r2 * r2.a;
		r1 = mo + bo;
		mo = r1 * (1.0 - (r3.a));
		bo = r3 * r3.a;
		r1 = mo + bo;
		mo = r1 * (1.0 - (r4.a));
		bo = r4 * r4.a;
		r1 = mo + bo;
		r1.a = r1.a * 1 - _Alpha;

		o.Albedo = r1.rgb * r1.a;
		o.Alpha = r1.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}