//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/PlasmaRainbow"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Colors("Colors", Range(4,128)) = 4
		_Color("Color", Color) = (1,1,1,1)
		_Offset("Offset", Range(4,128)) = 1
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
	float _Colors;
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
		o.color = v.color ;
	}

	inline float mod(float x,float modu)
	{
		return x - floor(x * (1.0 / modu)) * modu;
	}

	float3 rainbow(float t) {
		t = mod(t,1.0);
		float tx = t * _Colors;

		float r = clamp(tx - 4.0, 0.0, 1.0) + clamp(2.0 - tx, 0.0, 1.0);
		float g = tx < 2.0 ? clamp(tx, 0.0, 1.0) : clamp(4.0 - tx, 0.0, 1.0);
		float b = tx < 4.0 ? clamp(tx - 2.0, 0.0, 1.0) : clamp(6.0 - tx, 0.0, 1.0);
		return float3(r, g, b);
	}

	float3 plasma(float2 uv)
	{

_TimeX=_Time.y;
		float a = 1.1 + _TimeX * 2.25 + _Offset;
		float b = 0.5 + _TimeX * 1.77 + _Offset;
		float c = 8.4 + _TimeX * 1.58 + _Offset;
		float d = 610 + _TimeX * 2.03 + _Offset;
		float x1 = 2.0*uv.x;
		float n = sin(a + x1) + sin(b - x1) + sin(c + 2.0 * uv.y) + sin(d + 5.0 * uv.y);
		n = mod(((5.0 + n) / 5.0), 1.0);
		float4 nx = tex2D(_MainTex,uv);
		n += nx.r * 0.2 + nx.g * 0.4 + nx.b * 0.2;

		return rainbow(n);
	}


	void surf(Input IN, inout SurfaceOutput o)
	{

		float alpha = tex2D(_MainTex, IN.uv_MainTex).a;
		float4 c = float4(plasma(IN.uv_MainTex), alpha - _Alpha)*IN.color;

		o.Albedo = c.rgb * c.a;
		o.Alpha = c.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}