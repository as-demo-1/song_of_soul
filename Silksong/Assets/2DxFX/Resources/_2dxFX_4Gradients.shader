// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/4Gradients" 
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_Color ("_Color", Color) = (1,1,1,1)
_Color1 ("_Color1", Color) = (1,1,1,1)
_Color2 ("_Color2", Color) = (1,1,1,1)
_Color3 ("_Color3", Color) = (1,1,1,1)
_Color4 ("_Color4", Color) = (1,1,1,1)
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
float4 _Color;
float4 _Color1;
float4 _Color2;
float4 _Color3;
float4 _Color4;
float _Alpha;

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
float4 c = tex2D(_MainTex,i.texcoord)*i.color;
float alpha = c.a;
float2 uv=i.texcoord.xy;
float4 colorA = lerp(_Color3,_Color4,uv.x*1.3);
float4 colorB = lerp(_Color1,_Color2,uv.x*1.3);
float4 colorC = lerp(colorA,colorB,uv.y*1.3);
c = lerp(c,colorC,colorC.a);
c.a= (alpha)-_Alpha;
return c;
}

ENDCG
}
}
Fallback "Sprites/Default"
}