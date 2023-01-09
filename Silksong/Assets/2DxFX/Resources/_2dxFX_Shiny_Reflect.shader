// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/Shiny_Reflect"
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_MainTex2 ("Base (RGB)", 2D) = "white" {}
_Color ("_Color", Color) = (1,1,1,1)
_Distortion ("Distortion", Range(0,1)) = 0
_Value2 ("Value 2", Range(0,1)) = 0
_Value3 ("Value 3", Range(0,1)) = 0
_Value4 ("Value 4", Range(0,1)) = 0
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
sampler2D _MainTex2;
float _Distortion;
float _Value2;
float _Value3;
float _Value4;
float _Value5;
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
	

float4 frag (v2f i) : COLOR
{

float4 tex = tex2D(_MainTex, i.texcoord)*i.color;
float4 tex2 = tex2D(_MainTex2, i.texcoord)*i.color;
float3 lum = float3(0.299, 0.587, 0.114);
float x=dot( tex.rgb, lum).r;
float t=tex2.r+x*_Value5;

t=smoothstep(t,t+_Value2, _Distortion+_Value2*0.5);
t-=1-t;
t=1-abs(t);
t=abs(t);
t*=_Value3;
tex.rgb+=float3(t,t,t);
tex.a=tex.a*(1.0-_Alpha);
tex2=float4(1,1,1,t*tex.a);
tex=lerp(tex,tex2,_Value4);
return tex;	
	
}

ENDCG
}
}
Fallback "Sprites/Default"

}