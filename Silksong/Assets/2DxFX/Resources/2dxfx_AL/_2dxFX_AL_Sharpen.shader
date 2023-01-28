//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Sharpen"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Distortion("Distortion", Range(0,1)) = 0
		_Color("_Color", Color) = (1,1,1,1)
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

	inline float4 sharp(float2 uv)
	{
		float r = 1.0 / 256.0;
		float strength = 9.0 * _Distortion;

		float4 c0 = tex2D(_MainTex,uv);
		float4 c1 = tex2D(_MainTex,uv - float2(r,.0));
		float4 c2 = tex2D(_MainTex,uv + float2(r,.0));
		float4 c3 = tex2D(_MainTex,uv - float2(.0,r));
		float4 c4 = tex2D(_MainTex,uv + float2(.0,r));
		float4 c5 = c0 + c1 + c2 + c3 + c4; c5 *= 0.2;
		float4 mi = min(c0,c1); mi = min(mi,c2); mi = min(mi,c3); mi = min(mi,c4);
		float4 ma = max(c0,c1); ma = max(ma,c2); ma = max(ma,c3); ma = max(ma,c4);
		float4 rt = clamp(mi,(strength + 1.0)*c0 - c5*strength,ma);
		return float4(rt.rgb,c0.a);
	}

	void surf(Input IN, inout SurfaceOutput o)
	{

		float4 col = sharp(IN.uv_MainTex)*IN.color;
		col.a = col.a * 1 - _Alpha;
		o.Albedo = col.rgb * col.a;
		o.Alpha = col.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}