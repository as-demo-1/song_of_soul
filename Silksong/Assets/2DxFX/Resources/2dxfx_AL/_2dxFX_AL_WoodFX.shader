//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/WoodFX"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
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
#pragma target 3.0

		sampler2D _MainTex;
	float _Distortion;
	float _Deep;
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
		o.color = v.color ;
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

	float4 Wood(float2 uv)
	{
		float2 tuv = uv;
		uv *= 10.0;
		float a = 46.1f;
		float n = sin(a + 2.0 * uv.x) + sin(a - 2.0 * uv.x) + sin(a + 2.0 * uv.y) + sin(a + 5.0 * uv.y);
		n = mod(((5.0 + n) / 5.0), 1.0)*_Deep;
		n += tex2D(_MainTex, tuv).r * 11.2 + tex2D(_MainTex, tuv).g * 8.4 + tex2D(_MainTex, tuv).b * 4.2;
		return rainbow(n*_Deep);
	}


	void surf(Input IN, inout SurfaceOutput o)
	{

		float2 uv = IN.uv_MainTex;
		float4 tex = tex2D(_MainTex, IN.uv_MainTex + float2(sin(_Distortion * 64) / 512,0))* IN.color;
		float lum = dot(tex.rgb, float3(.22, .17, .571));
		float rate = 0.3;
		float r = lum*0.7;
		r += 0.15;
		if (r>0.6) r = 0.6;
		if (r<0.3) r = 0.3;

		float4 sortie = Wood(IN.uv_MainTex);
		sortie -= Wood(IN.uv_MainTex + float2(-0.05,0.02)) / 2.0;
		float3 r2 = r - (1 - sortie.a) / 8;
		float3 tex2;
		r2.r -= 0.1;
		r2.g -= 0.35;
		r2.b -= 0.5;
		r2 += 0.35;
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