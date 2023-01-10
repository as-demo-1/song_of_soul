//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Pixel8bitsGB"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("_Color", Color) = (1,1,1,1)
		_Size("Size", Range(0,1)) = 0
		_Offset("Offset", Range(0,1)) = 0
		_Offset2("Offset2", Range(0,1)) = 0
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
	float _Size;
	float _Offset;
	float _Offset2;
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

	float compare(float3 a, float3 b)
	{
		a = a*a*a;
		b = b*b*b;
		float3 diff = (a - b);
		return dot(diff, diff);
	}

	inline float mod(float x,float modu)
	{
		return x - floor(x * (1.0 / modu)) * modu;
	}

	void surf(Input IN, inout SurfaceOutput o)
	{

		float2 q = IN.uv_MainTex;
		float2 pixelSize = 64 * _Size;
		float2 c = floor(q * pixelSize) / pixelSize;
		float4 src4 = tex2D(_MainTex, c)* IN.color;
		float3 src = src4.rgb*_Offset2;
		float alpha = src4.a;
		if (alpha<0.95) alpha = 0;

		float3 dst0 = float3(0,0,0);
		float3 dst1 = float3(0,0,0);
		float best0 = 1e3;
		float best1 = 1e3;

#define TCOLOR(R, G, B) { const float3 tst = float3(R, G, B); float err = compare(src, tst); if (err < best0) { best1 = best0; dst1 = dst0; best0 = err; dst0 = tst; } }
		TCOLOR(0.68, 0.79, 0.27);
		TCOLOR(0.47, 0.69, 0.42);
		TCOLOR(0.13, 0.43, 0.37);
		TCOLOR(0.03, 0.16, 0.33);
#undef TCOLOR

		best0 = sqrt(best0); best1 = sqrt(best1);
		float4 r = float4(mod(c.x + c.y, 2.0) >(1 + best1 / (best0 + best1)) ? dst1 : dst0, 1.0);
		r.a = r.a*alpha*(1 - _Alpha);

		o.Albedo = r.rgb * r.a;
		o.Alpha = r.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}