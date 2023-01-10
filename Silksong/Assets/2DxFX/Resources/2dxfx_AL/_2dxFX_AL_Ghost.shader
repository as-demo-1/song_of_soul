//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Ghost"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("_Color", Color) = (1,1,1,1)
		_ClipLeft("Clipping Left", Range(0,1)) = 1
		_ClipRight("Clipping Right", Range(0,1)) = 1
		_ClipUp("Clipping Up", Range(0,1)) = 1
		_ClipDown("Clipping Down", Range(0,1)) = 1
		_offset("offset", Range(0,1)) = 1
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
	float4 _Color;
	float _Alpha;
	float _offset;
	float _ClipLeft;
	float _ClipRight;
	float _ClipUp;
	float _ClipDown;

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

	void surf(Input IN, inout SurfaceOutput o)
	{

		float4 mainColor = tex2D(_MainTex, IN.uv_MainTex)*IN.color;
		float alpha = mainColor.a;
		float c1 = 1;
		float noffset = _offset;
		if (IN.uv_MainTex.y > _ClipUp)     c1 = saturate(((1 + noffset) / (1 - _ClipUp))*(1 - IN.uv_MainTex.y) - noffset);
		if (IN.uv_MainTex.y < 1 - _ClipDown) c1 *= saturate(((1 + noffset) / (1 - _ClipDown))*IN.uv_MainTex.y - noffset);
		if (IN.uv_MainTex.x > _ClipRight)  c1 *= saturate(((1 + noffset) / (1 - _ClipRight))*(1 - IN.uv_MainTex.x) - noffset);
		if (IN.uv_MainTex.x < 1 - _ClipLeft) c1 *= saturate(((1 + noffset) / (1 - _ClipLeft))*IN.uv_MainTex.x - noffset);
		mainColor.a = (alpha*c1) - _Alpha;
		float4 c = float4(mainColor.rgb,mainColor.a);

		o.Albedo = c.rgb * c.a;
		o.Alpha = c.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}