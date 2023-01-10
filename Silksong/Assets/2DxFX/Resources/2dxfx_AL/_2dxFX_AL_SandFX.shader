//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/SandFX"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("_Color", Color) = (1,1,1,1)
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
	float _Alpha;
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
	inline float rand(float2 co) {
		return frac(sin(dot(co.xy,float2(12.9898,78.233))) * 43758.5453);
	}
	void surf(Input IN, inout SurfaceOutput o)
	{

		float4 Alpha = tex2D(_MainTex, IN.uv_MainTex + float2(sin(IN.uv_MainTex.y*125.82*_Distortion / 3) / 140,sin(IN.uv_MainTex.y*31.4*_Distortion / 3) / 40))*IN.color;
		float4 res;
		float lum = dot(Alpha.rgb, float3(.222, .707, .071));
		float noise = lerp(lum,rand(IN.uv_MainTex.xy),_Distortion / 3);
		if (noise>0.6) noise = 0.6;
		if (noise<0.3) noise = 0.3;
		res.rgb = noise;
		res.r += 0.5;
		res.g += 0.3;
		res.b -= 0.3;
		res.rgb = lerp(Alpha.rgb,res.rgb,_Distortion);
		float4 c = float4(res.rgb, Alpha.a*(1.0 - _Alpha));


		o.Albedo = c.rgb * c.a;
		o.Alpha = c.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}