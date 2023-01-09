//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Mystic_Distortion"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Pitch("Pitch", Range(0,0.5)) = 0
		_OffsetX("OffsetX", Range(0,128)) = 0
		_OffsetY("OffsetY", Range(0,128)) = 0
		_DistanceX("DistanceX", Range(0,1)) = 0
		_DistanceY("DistanceY", Range(0,1)) = 0
		_WaveTimeX("WaveTimeX", Range(0,360)) = 0
		_WaveTimeY("WaveTimeY", Range(0,360)) = 0
		_Color("Tint", Color) = (1,1,1,1)
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
	float _OffsetX;
	float _OffsetY;
	float _Pitch;
	float4 _Color;
	float _DistanceX;
	float _DistanceY;
	float _WaveTimeX;
	float _WaveTimeY;
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

		float2 p = IN.uv_MainTex;
		p.x = p.x + sin(p.y*_OffsetX + _WaveTimeX)*_DistanceX;
		p.y = p.y + cos(p.x*_OffsetY + _WaveTimeY)*_DistanceY;
		float2 m = float2(0.5, 0.5);
		float2 d = p - m;
		float  r = sqrt(dot(d, d));
		float power = 4.44289334 * (_Pitch - 0.5);
		float bind = m.y;
		float2 uv;
		uv = m + normalize(d) * atan(r * -power * 10.0) * bind / atan(-power * bind * 10.0);
		float4 mainColor = tex2D(_MainTex, uv)* IN.color;
		mainColor.a -= _Alpha;
		o.Albedo = mainColor.rgb * mainColor.a;
		o.Alpha = mainColor.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}