//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/PlasmaShield"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Size("Size", Range(4,128)) = 4
		_Color("Tint", Color) = (1,1,1,1)
		_ColorX("Tint", Color) = (1,1,1,1)
		_Offset("Offset", Range(4,128)) = 4
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
	float _Offset;
	float _Alpha;
	float4 _Color;
	float4 _ColorX;
	float _Size;
	float _TimeX;



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

	inline float mod(float x,float modu)
	{
		return x - floor(x * (1.0 / modu)) * modu;
	}

	float4 rainbow(float t)
	{
		t = mod(t,1.0);
		float tx = t * _Size;
		float r = clamp(tx - 2.0, 0.0, 1.0) + clamp(2.0 - tx, 0.0, 1.0);
		return float4(_ColorX.r, _ColorX.g, _ColorX.b,1 - r + (1. - _ColorX.a));
	}

	float4 plasma(float2 uv)
	{
	_TimeX=_Time.y;
		float a = 1.1 + _TimeX * 2.25 + _Offset;
		float x1 = 2.0*uv.x;
		float n = sin(a + x1) + sin(a - x1) + sin(a + 2.0 * uv.y) + sin(a + 5.0 * uv.y);
		n = mod(((5.0 + n) / 5.0), 1.0);
		float3 nx = tex2D(_MainTex, uv);
		n += nx.r * 0.2 + nx.g * 0.4 + nx.b * 0.2;
		return rainbow(n);
	}



	void surf(Input IN, inout SurfaceOutput o)
	{

		float2  p = IN.uv_MainTex;
		float4 c = tex2D(_MainTex,p) * IN.color;
		c.a = c.a*(1 - _Alpha);
		float alpha = tex2D(_MainTex, IN.uv_MainTex).a;
		float4 sortie = plasma(IN.uv_MainTex);
		sortie.a = sortie.a*alpha - _Alpha;
		float4 col = sortie*IN.color;
		o.Albedo = col.rgb * col.a;
		o.Alpha = col.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}