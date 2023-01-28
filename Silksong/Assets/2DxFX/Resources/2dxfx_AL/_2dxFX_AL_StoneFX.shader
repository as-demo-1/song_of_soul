//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/StoneFX"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Distortion("Distortion", Range(0,1)) = 0
		_Alpha("Alpha", Range(0,1)) = 1.0
		_Deep("Alpha", Range(0,1)) = 1.0
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
	float _Alpha;
	float _Deep;

	struct Input
	{
		float2 uv_MainTex;
		float4 color;
	};

	void vert(inout appdata_full v, out Input o)
	{
		v.vertex = UnityPixelSnap(v.vertex);
		UNITY_INITIALIZE_OUTPUT(Input, o);
		o.color = v.color;
	}

	inline float mod(float x,float modu)
	{
		return x - floor(x * (1.0 / modu)) * modu;
	}

	float4 rainbow(float t)
	{
		t = mod(t,1.0);
		float tx = t * 6.0;

		float r = clamp(tx - 2.0, 0.0, 1.0) + clamp(2.0 - tx, 0.0, 1.0);

		return float4(1.0, 1.0, 1.0,r);
	}

	float4 plasma(float2 uv)
	{
		float2 tuv = uv;
		uv *= 15.0;
		float a = 1.1 + 20 * 2.25;
		float n = sin(a + 2.0 * uv.x) + sin(a - 2.0 * uv.x) + sin(a + 2.0 * uv.y) + sin(a + 5.0 * uv.y);
		n = mod(((5.0 + n) / 5.0), 1.0);
		n += tex2D(_MainTex, tuv).r * 11.2 + tex2D(_MainTex, tuv).g * 8.4 + tex2D(_MainTex, tuv).b * 4.2;

		return rainbow(n*_Deep);
	}



	void surf(Input IN, inout SurfaceOutput o)
	{

		float2 uv = IN.uv_MainTex;
		float4 tex = tex2D(_MainTex, IN.uv_MainTex + float2(sin(_Distortion * 64) / 512,0));
		float lum = (tex.r + tex.b + tex.g) / 3;
		float a = lum;
		float rate = 0.2;
		float r = a*(1 - rate);
		r = r + (1 * (rate / 2));
		if (r>0.6) r = 0.6;
		if (r<0.2) r = 0.2;
		float4 sortie = plasma(IN.uv_MainTex);
		sortie += plasma(float2(IN.uv_MainTex.y,IN.uv_MainTex.x));
		float3 r2 = r - (1 - sortie.a) / 8;
		tex.rgb = lerp(tex.rgb,r2,_Distortion);
		float4 c = float4(tex.rgb,tex.a * 1 - _Alpha);

		o.Albedo = c.rgb * c.a;
		o.Alpha = c.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Sprites/Default"
}