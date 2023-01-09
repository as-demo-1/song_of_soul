//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/BlackHole"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("_Color", Color) = (1,1,1,1)
		_Size("Size", Range(0,1)) = 0
		_Distortion("Distortion", Range(0,1)) = 0
		_Hole("Hole", Range(0,1)) = 0
		_Speed("Speed", Range(0,1)) = 0
		_Alpha("Alpha", Range(0,1)) = 1.0
		_Color("ColorX", Color) = (1,1,1,1)
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
	float4 _ColorX;
	float _Size;
	float _Distortion;
	float _Hole;
	float _Speed;
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
	float4 hole(sampler2D tex, float2 uv, float time)
	{
		float radius = 0.5;
		float2 center = float2(0.5,0.5);
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
		_Speed *= 5.0f;


		float2 uv = (IN.uv_MainTex - float2(0.5,0.5))*1.246;
		float sinX = sin(_Speed * _Time);
		float cosX = cos(_Speed * _Time);
		float sinY = sinX;
		float2x2 rotationMatrix = float2x2(cosX, -sinX, sinY, cosX);
		uv.xy = mul(uv, rotationMatrix) + float2(0.5,0.5);
		float dist = 1.0 - smoothstep(_Hole,_Hole + 0.15, length(float2(0.5,0.5) - uv));
		float dista = 1.0 - smoothstep(0.25,0.5, length(float2(0.5,0.5) - uv));
		float4 finalColor = hole(_MainTex, uv, _Distortion);
		finalColor.rgb *= 1 - dist;
		finalColor.a = finalColor.a*(1 - _Alpha);
		finalColor.a *= dista*(1 - dist);








		o.Albedo = finalColor.rgb * finalColor.a;
		o.Alpha = finalColor.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}