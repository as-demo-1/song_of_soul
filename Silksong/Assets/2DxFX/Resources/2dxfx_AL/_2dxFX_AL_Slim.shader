//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Slim"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	[HideInInspector] _MainTex2("Pattern (RGB)", 2D) = "white" {}
	_Color("_Color", Color) = (1,1,1,1)
		_Distortion("Distortion", Range(0,1)) = 0
		_Alpha("Alpha", Range(0,1)) = 1.0
		_Speed("Speed", Range(0,1)) = 1.0
		EValue("EValue", Range(0,1)) = 1.0
		Light("Light", Range(0,1)) = 1.0
		TurnToLiquid("TurnToLiquid", Range(0,1)) = 1.0
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
	sampler2D _MainTex2;
	float _Distortion;
	float _Alpha;
	float _Speed;
	float EValue;
	float4 _Color;
	float Light;
	float TurnToLiquid;

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
		float c1 = 1;
		float noffset = TurnToLiquid*sin(p.x * 16 * (TurnToLiquid + 1)) / 2;
		float _ClipUp = 1 - TurnToLiquid * 2;
		float Mix = TurnToLiquid + _Distortion;
		c1 = saturate(((1 + noffset) / (1 - _ClipUp + 0.04))*(1 - IN.uv_MainTex.y) - noffset);
		float r = 1 - c1;
		float2 p2 = IN.uv_MainTex.xy;
		p2.y += TurnToLiquid - 0.2;
		p2 /= 2 / (1 + Mix / 8);


		float4 col2 = tex2D(_MainTex2,p2);
		col2 *= Mix * 20;
		//p+=float2(col2.r/32,col2.g/32);
		float t = 1 + (_Time * 100);

		float2 mx = col2.r / 32;
		p += float2(mx.x,mx.y);
		p.x -= Mix / 4;
		p.x += noffset / 4;
		p.y -= _Distortion / 2.3;
		float4 col = tex2D(_MainTex,p)*IN.color;
		col.rgb += r / 2;
		col.g += col2.g / 8;
		col.rb -= col2.rb / 8;
		float alpha = 1 - ((0.4 + p.y)*TurnToLiquid * 2);
		float4 c = float4(col.rgb,col.a*alpha*(1 - _Alpha));

		o.Albedo = c.rgb * c.a;
		o.Alpha = c.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Sprites/Default"
}