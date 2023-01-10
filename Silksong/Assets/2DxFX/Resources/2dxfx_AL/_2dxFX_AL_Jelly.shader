//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Jelly"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("_Color", Color) = (1,1,1,1)
		_Distortion("Distortion", Range(0,1)) = 0
		_RandomPos("RandomPos", Range(0,1)) = 0
		_Inside("Inside", Range(0,1)) = 0
		_Alpha("Alpha", Range(0,1)) = 1.0
		_Speed("Speed", Range(0,1)) = 1.0
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
	float _RandomPos;
	float _Inside;
	float _Alpha;
	float _Speed;
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

	void surf(Input IN, inout SurfaceOutput o)
	{

		float2 uv = IN.uv_MainTex;
		float time = _Time*_Speed * 200 + _RandomPos;

		uv.x += (sin(uv.y + time) * 0.019*_Distortion);
		uv.y += (cos(uv.x + time) * 0.009*_Distortion);
		uv.x = lerp(uv.x,IN.uv_MainTex.x,1 - IN.uv_MainTex.y);
		uv.y = lerp(uv.y,IN.uv_MainTex.y,1 - IN.uv_MainTex.y);

		float4 rcol = tex2D(_MainTex, uv)* IN.color;
		uv = IN.uv_MainTex;
		uv.x += (sin(uv.y + time) * 0.019*_Distortion*_Inside);
		uv.y += (cos(uv.x + time) * 0.009*_Distortion*_Inside);
		uv.x = lerp(uv.x,IN.uv_MainTex.x,1 - IN.uv_MainTex.y);
		uv.y = lerp(uv.y,IN.uv_MainTex.y,1 - IN.uv_MainTex.y);

		float2 scaleCenter = float2(0.5f, 0.5f);
		uv = (uv - scaleCenter) * (_Inside)+scaleCenter;

		float4 rcol2 = (tex2D(_MainTex, uv)* IN.color);
		rcol.rgb = lerp(rcol.rgb,rcol.rgb / 2,rcol2.a);
		rcol2.rgb /= 2 + (1 - _Inside);
		rcol.rgb += rcol2.rgb*rcol2.a;
		rcol.a = rcol.a * 1 - _Alpha;

		o.Albedo = rcol.rgb * rcol.a;
		o.Alpha = rcol.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}