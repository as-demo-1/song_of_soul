// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/SandFX"
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_Color ("_Color", Color) = (1,1,1,1)
_Distortion ("Distortion", Range(0,1)) = 0
_Alpha ("Alpha", Range (0,1)) = 1.0
// required for UI.Mask
_StencilComp ("Stencil Comparison", Float) = 8
_Stencil ("Stencil ID", Float) = 0
_StencilOp ("Stencil Operation", Float) = 0
_StencilWriteMask ("Stencil Write Mask", Float) = 255
_StencilReadMask ("Stencil Read Mask", Float) = 255
_ColorMask ("Color Mask", Float) = 15

}

SubShader
{

Tags {"Queue"="Transparent" "IgnoreProjector"="true" "RenderType"="Transparent"}
ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Cull Off

// required for UI.Mask
Stencil
{
Ref [_Stencil]
Comp [_StencilComp]
Pass [_StencilOp] 
ReadMask [_StencilReadMask]
WriteMask [_StencilWriteMask]
}


Pass
{

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#pragma target 3.0
#include "UnityCG.cginc"

struct appdata_t
{
float4 vertex   : POSITION;
float4 color    : COLOR;
float2 texcoord : TEXCOORD0;
};

struct v2f
{
float2 texcoord  : TEXCOORD0;
float4 vertex   : SV_POSITION;
float4 color    : COLOR;
};


sampler2D _MainTex;
float _Distortion;
float _Alpha;
float4 _Color;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}
	
	
inline float rand(float2 co){
    return frac(sin(dot(co.xy,float2(12.9898,78.233))) * 43758.5453);
}
float4 frag (v2f i) : COLOR
{

float4 Alpha = tex2D(_MainTex, i.texcoord+float2(sin(i.texcoord.y*125.82*_Distortion/3)/140,sin(i.texcoord.y*31.4*_Distortion/3)/40))*i.color;
float4 res=float4(0,0,0,0);
float lum=dot(Alpha.rgb, float3(.222, .707, .071));
float noise=lerp(lum,rand(i.texcoord.xy),_Distortion/3);
if (noise>0.6) noise=0.6;
if (noise<0.3) noise=0.3;
res.rgb = noise;
res.r+=0.5;
res.g+=0.3;
res.b-=0.3;
res.rgb=lerp(Alpha.rgb,res.rgb,_Distortion);   
return float4(res.rgb, Alpha.a*(1.0-_Alpha));	
	
}

ENDCG
}
}
Fallback "Sprites/Default"

}