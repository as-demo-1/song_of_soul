//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Blur"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("_Color", Color) = (1,1,1,1)
		_Distortion("Distortion", Range(0,1)) = 0
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
	float _Distortion;
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

		float stepU = 0.00390625f * _Distortion;
		float stepV = stepU;

		float3x3 gaussian = float3x3(1.0,2.0,1.0,2.0,4.0,2.0,1.0,2.0,1.0);
		float4 result = float4(0,0,0,0);
		float4 Alpha = tex2D(_MainTex, IN.uv_MainTex);
		float2 texCoord = float2(0,0);

		texCoord = IN.uv_MainTex.xy + float2(-stepU, -stepV); result += tex2D(_MainTex,texCoord);
		texCoord = IN.uv_MainTex.xy + float2(-stepU, 0); result += 2.0 * tex2D(_MainTex,texCoord);
		texCoord = IN.uv_MainTex.xy + float2(-stepU, stepV); result += tex2D(_MainTex,texCoord);
		texCoord = IN.uv_MainTex.xy + float2(0, -stepV); result += 2.0 * tex2D(_MainTex,texCoord);
		texCoord = IN.uv_MainTex.xy; result += 4.0 * tex2D(_MainTex,texCoord);
		texCoord = IN.uv_MainTex.xy + float2(0, stepV); result += 2.0 * tex2D(_MainTex,texCoord);
		texCoord = IN.uv_MainTex.xy + float2(stepU, -stepV); result += tex2D(_MainTex,texCoord);
		texCoord = IN.uv_MainTex.xy + float2(stepU, 0); result += 2.0* tex2D(_MainTex,texCoord);
		texCoord = IN.uv_MainTex.xy + float2(stepU, -stepV); result += tex2D(_MainTex,texCoord);

		float4 r = float4(0,0,0,0);
		r = result*0.0625;
		r.a *= Alpha.a*(1.0 - _Alpha);
		r = r*IN.color;

		o.Albedo = r.rgb * r.a;
		o.Alpha = r.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}