//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/EnergyBar"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("Tint", Color) = (1,1,1,1)
		_Value1("_Value1", Range(0,1)) = 1
		_Value2("_Value2", Range(0,1)) = 1
		_Value3("_Value3", Range(0,1)) = 1
		_Value4("_Value4", Range(0,1)) = 1
		_Value5("_Value5", Range(0,1)) = 1
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
	float4 _Color;
	float _Size;
	float _Value1;
	float _Value2;
	float _Value3;
	float _Value4;
	float _Value5;
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

	void surf(Input IN, inout SurfaceOutput o)
	{

		float2  p = IN.uv_MainTex;
		float4 c = tex2D(_MainTex,p) * IN.color;
		c.a = c.a*(1 - _Alpha);
		float2 uv = IN.uv_MainTex;
		float4 mainColor = tex2D(_MainTex, uv)* IN.color;
		float energy = smoothstep(_Value1 - _Value2,_Value1 + _Value2, uv.x);
		float xx = smoothstep(0.15 - 0.1,0.15 + 0.1, uv.x)*_Value1;
		float3 C1 = float3(1,0,0);
		float3 C2 = mainColor.rgb;
		C1 = lerp(mainColor.rgb,C1,_Value4);
		C1 = lerp(C1,mainColor.rgb,xx);
		float3 CR = lerp(C1,C2,_Value1);
		float4 CRA = float4(CR,mainColor.a);
		mainColor = lerp(CRA,mainColor - float4(_Value3,_Value3,_Value3,1 - _Value5),energy);
		mainColor.a = mainColor.a - _Alpha;

		o.Albedo = mainColor.rgb * mainColor.a;
		o.Alpha = mainColor.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}