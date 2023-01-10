//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Clipping"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("Tint", Color) = (1,1,1,1)
		_ClipLeft("Clipping Left", Range(0,1)) = 1
		_ClipRight("Clipping Right", Range(0,1)) = 1
		_ClipUp("Clipping Up", Range(0,1)) = 1
		_ClipDown("Clipping Down", Range(0,1)) = 1
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
	float _Size;
	float _ClipLeft;
	float _ClipRight;
	float _ClipUp;
	float _ClipDown;
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

	void surf(Input IN, inout SurfaceOutput o)
	{

		float4 mainColor = tex2D(_MainTex,IN.uv_MainTex)*IN.color;
		if (IN.uv_MainTex.y > _ClipUp) mainColor = float4(0,0,0,0);
		if (IN.uv_MainTex.y < 1 - _ClipDown) mainColor = float4(0,0,0,0);
		if (IN.uv_MainTex.x > _ClipRight) mainColor = float4(0,0,0,0);
		if (IN.uv_MainTex.x < 1 - _ClipLeft) mainColor = float4(0,0,0,0);
		mainColor.a = mainColor.a - _Alpha;
		
		o.Albedo = mainColor.rgb * mainColor.a;
		o.Alpha = mainColor.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}