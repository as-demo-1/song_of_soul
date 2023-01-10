//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Flame"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
_MainTex2 ("Base (RGB)", 2D) = "white" {}
_Speed("_Speed", Range(4,128)) = 4
_Intensity("_Intensity", Range(4,128)) = 4
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
#pragma target 3.0
sampler2D _MainTex;
sampler2D _MainTex2;
float _Speed;
float _Intensity;
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

void surf(Input IN, inout SurfaceOutput o)
{

 
    float2 uv = IN.uv_MainTex;
    float2 _uv = uv;
	float _TimeX=_Time*128*_Speed;

    uv -= float2(0.5,0.5);
    float2 centerUV = uv;
    
    
    float2 offset = float2(0.0, -_TimeX * 0.15);
    
    // flame thickness
    float2 uv2= IN.uv_MainTex +offset;
	float2 uv3= IN.uv_MainTex +offset*1.5;
	uv2.y/=16;
	uv3.y/=12;
	float flame = 1.3 - length(uv.x) * 3.0;
	float4 t3 =  tex2D(_MainTex2,uv3);
	float4 t2 = tex2D(_MainTex2,uv2);
	float variationH = t3.g-t2.g;
    uv2.x += IN.uv_MainTex.y*cos(_TimeX)/8;
    float4 t =  tex2D(_MainTex, float2(uv2.x, IN.uv_MainTex.y));
	flame *= smoothstep(1., variationH * _Intensity, _uv.y);
	flame = clamp(flame, 0.0, 1.0);
	flame = pow(flame, 3.);
	flame /= smoothstep(1.1, -0.1, _uv.y*t.a);
    // colors

    flame *= t.a;

	// colors
    float4 col = lerp(float4(1.0, 1., 0.0, 0.0), float4(1.0, 1.0, .6, 0.0), flame);
    col = lerp(float4(1.0, .0, 0.0, 0.0), col, smoothstep(0.0, 1.6, flame));
	col = lerp(float4(0.0, .0, 1.0, 0.0), col, smoothstep(0.0, 0.7, flame));
	
	t2=col*1.2;
	t2.a = t2.r*flame*_Alpha;
	t2.rgb= t2*IN.color;

	o.Albedo = t2.rgb * t2.a;
	o.Alpha = t2.a;

clip(o.Alpha-0.05);
}
ENDCG
}

Fallback "Sprites/Default"
}