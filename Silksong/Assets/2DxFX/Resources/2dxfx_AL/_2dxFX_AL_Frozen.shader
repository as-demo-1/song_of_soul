//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Frozen"
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
		_Color("Tint", Color) = (1,1,1,1)
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
	float hardLight(float s, float d)
	{
		return (s < 0.5) ? 2.0 * s * d : 1.0 - 2.0 * (1.0 - s) * (1.0 - d);
	}

	float3 hardLight(float3 s, float3 d)
	{
		float3 c;
		c.x = hardLight(s.x,d.x);
		c.y = hardLight(s.y,d.y);
		c.z = hardLight(s.z,d.z);
		return c;
	}

	void surf(Input IN, inout SurfaceOutput o)
	{

		float speed = _Time * 2;
		float2 uv = IN.uv_MainTex;
		uv = uv*(1 - (_Value2*0.4)) + float2(_Value2*0.2,_Value2*0.2);
		float4 t2 = tex2D(_MainTex2, uv);
		uv = uv*(1 - (_Value2*0.4)) + float2(_Value2*0.2,_Value2*0.2);
		float4 t3 = tex2D(_MainTex2, uv) * 2;
		uv = IN.uv_MainTex + float2(-_Value2 - speed,0);
		float4 t4 = tex2D(_MainTex2, uv + float2(t2.r,t2.r))*0.5;
		float tx = t3.b;
		tx = lerp(0,t3.b*0.015,_Value2);
		float4 td = tex2D(_MainTex, IN.uv_MainTex)*IN.color;
		float4 t = tex2D(_MainTex, IN.uv_MainTex + float2(tx,tx))*IN.color;
		t2.a = t.a;
		t2.rgb = float3(t2.r / 10,t2.r / 1.5,t2.r);
		float3 g = (t.r + t.g + t.b) / 3;
		float3 r = smoothstep(_Value1,_Value1 + 0.1,g) + .2;
		t.rgb = lerp(t.rgb,hardLight(g,t2.rgb) + t2.b*r*t3.b + t4.g,_Value2);
		float4 c = float4(t.rgb,t.a*(1 - _Alpha));
		o.Albedo = c.rgb * c.a;
		o.Alpha = c.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}