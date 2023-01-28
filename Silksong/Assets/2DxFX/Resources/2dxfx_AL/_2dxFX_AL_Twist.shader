//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Twist"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Distortion("Distortion", Range(0,1)) = 0
		_PosX("PosX", Range(0,1)) = 0
		_PosY("PosY", Range(0,1)) = 0
		_Alpha("Alpha", Range(0,1)) = 1.0
		_Color("Color", Color) = (1,1,1,1)
		_ColorX("ColorX", Color) = (1,1,1,1)
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
	float _PosX;
	float _PosY;
	float4 _Color;
	float4 _ColorX;
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



	float4 twist(sampler2D tex, float2 uv, float time)
	{
		float radius = 0.5;
		float2 center = float2(_PosX,_PosY);
		float2 tc = uv - center;
		float dist = length(tc);
		if (dist < radius)
		{
			float percent = (radius - dist) / radius;
			float theta = percent * percent * (2.0 * sin(time)) * 8.0;
			float s = sin(theta);
			float c = cos(theta);
			tc = float2(dot(tc, float2(c, -s)), dot(tc, float2(s, c)));
		}
		tc += center;
		float4 color = tex2D(tex, tc);
		return color;
	}



	void surf(Input IN, inout SurfaceOutput o)
	{

		float2 uv = IN.uv_MainTex;
		float4 finalColor = twist(_MainTex, uv, _Distortion)* IN.color;
		finalColor.a *= (1 - _Alpha);
		o.Albedo = finalColor.rgb * finalColor.a;
		o.Alpha = finalColor.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Sprites/Default"
}