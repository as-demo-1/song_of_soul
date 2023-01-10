//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Cartoon"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("Tint", Color) = (1,1,1,1)
		_ColorLevel("ColorLevel", Range(0,1)) = 0
		_EdgeSize("EdgeSize", Range(0,1)) = 0
		_ColorB("ColorB", Range(0,1)) = 0
		_Size("Size", Range(0,1)) = 0
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
	float _ColorLevel;
	float _EdgeSize;
	float _ColorB;
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

	inline float4 edgeFilter(in int px, in int py,float2 uv)
	{

		float4 color = 0.0;
		float2 kUV = uv*256.0f;
		color += tex2D(_MainTex, (kUV + float2(px + 1, py + 1)) * 0.00390625f);
		color += tex2D(_MainTex, (kUV + float2(px    , py + 1)) * 0.00390625f);
		color += tex2D(_MainTex, (kUV + float2(px - 1, py + 1)) * 0.00390625f);
		color += tex2D(_MainTex, (kUV + float2(px + 1, py)) * 0.00390625f);
		color += tex2D(_MainTex, (kUV + float2(px    , py)) * 0.00390625f);
		color += tex2D(_MainTex, (kUV + float2(px - 1, py)) * 0.00390625f);
		color += tex2D(_MainTex, (kUV + float2(px + 1, py - 1)) * 0.00390625f);
		color += tex2D(_MainTex, (kUV + float2(px    , py - 1)) * 0.00390625f);
		color += tex2D(_MainTex, (kUV + float2(px - 1, py - 1)) * 0.00390625f);
		return color / 9.0;

	}

	void surf(Input IN, inout SurfaceOutput o)
	{

		float2 uv = IN.uv_MainTex;
		float4 tex = tex2D(_MainTex, uv);

		float4 color = 0;
		float2 kUV = uv*256.0f;
		color += tex2D(_MainTex, (kUV + float2(1 ,  1)) * 0.00390625f);
		color += tex2D(_MainTex, (kUV + float2(0 ,  1)) * 0.00390625f);
		color += tex2D(_MainTex, (kUV + float2(-1 ,  1)) * 0.00390625f);
		color += tex2D(_MainTex, (kUV + float2(1 ,  0)) * 0.00390625f);
		color += tex2D(_MainTex, (kUV)* 0.00390625f);
		color += tex2D(_MainTex, (kUV + float2(-1 ,  0)) * 0.00390625f);
		color += tex2D(_MainTex, (kUV + float2(1 , -1)) * 0.00390625f);
		color += tex2D(_MainTex, (kUV + float2(0 , -1)) * 0.00390625f);
		color += tex2D(_MainTex, (kUV + float2(-1 , -1)) * 0.00390625f);
		color /= 9.0;
		color *= IN.color;
		color[0] = floor(7.0 * color[0]) / _ColorLevel;
		color[1] = floor(7.0 * color[1]) / _ColorLevel;
		color[2] = floor(7.0 * color[2]) / _ColorLevel;
		float4 sum = abs(edgeFilter(0, 1, uv) - edgeFilter(0, -1, uv));
		sum += abs(edgeFilter(1, 0, uv) - edgeFilter(-1, 0, uv));
		sum /= 2.0;
		float edgsum = _EdgeSize + 0.05;
		if (length(sum) > edgsum) { color.rgb = 0.0; }
		float4 r = float4(color.rgb,tex.a * 1 - _Alpha);
		o.Albedo = r.rgb * r.a;
		o.Alpha = r.a;
		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}