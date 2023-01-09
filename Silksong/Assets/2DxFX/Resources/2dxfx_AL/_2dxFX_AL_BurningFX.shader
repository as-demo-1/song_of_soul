//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/BurningFX"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("Tint", Color) = (1,1,1,1)
		_Distortion("Distortion", Range(0,1)) = 0
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
	float _Distortion;
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

	inline float3 Burn(float _t)
	{
		float u = (0.860117757 + 1.54118254e-4*_t + 1.28641212e-7*_t*_t) / (1.0 + 8.42420235e-4*_t + 7.08145163e-7*_t*_t);
		float v = (0.317398726 + 4.22806245e-5*_t + 4.20481691e-8*_t*_t) / (1.0 - 2.89741816e-5*_t + 1.61456053e-7*_t*_t);
		float kuv = 2.0 * u - 8.0 * v + 4.0;
		float x = 3.0 * u / kuv;
		float y = 2.0 * v / kuv;
		float z = 1.0 - x - y;
		float Y = 1.0;
		float YY = Y / y;
		float X = YY * x;
		float Z = YY * z;
		float3 RGB = float3(X,Y,Z);
		RGB.x = RGB.x * pow(0.0006*_t*_Distortion, 4.0) / _Distortion;
		RGB.y = RGB.y * pow(0.0004*_t*_Distortion, 4.0) / _Distortion;
		RGB.z = RGB.z * pow(0.0004*_t*_Distortion, 4.0)*_Distortion;

		return RGB;
	}

	void surf(Input IN, inout SurfaceOutput o)
	{
		float2 uv = IN.uv_MainTex;
		float4 noise = tex2D(_MainTex, uv)* IN.color;
		float lum = dot(noise.rgb, float3 (0.2126, 0.7152, 0.0722));
		float3 c = Burn(lum * 4000.0);
		c = c + noise.rgb;
		float4 r = float4(c,noise.a * 1 - _Alpha);
		o.Albedo = r.rgb * r.a;
		o.Alpha = r.a;
		clip(o.Alpha - 0.05);
	}

	ENDCG
	}

		Fallback "Transparent/VertexLit"
}