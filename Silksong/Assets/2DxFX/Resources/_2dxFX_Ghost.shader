// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/Ghost"
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_Color ("_Color", Color) = (1,1,1,1)
_ClipLeft ("Clipping Left", Range(0,1)) = 1
_ClipRight ("Clipping Right", Range(0,1)) = 1
_ClipUp ("Clipping Up", Range(0,1)) = 1
_ClipDown ("Clipping Down", Range(0,1)) = 1
_offset ("offset", Range(0,1)) = 1
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
float _Alpha;
float _offset;
float _ClipLeft;
float _ClipRight;
float _ClipUp;
float _ClipDown;


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
float4 mainColor = tex2D(_MainTex, i.texcoord)*i.color;
float alpha=mainColor.a;
float c1=1;
float noffset=_offset;
if ( i.texcoord.y > _ClipUp)      c1= saturate(((1+noffset)/(1-_ClipUp))*(1-i.texcoord.y)-noffset);
if ( i.texcoord.y < 1-_ClipDown) c1*= saturate(((1+noffset)/(1-_ClipDown))*i.texcoord.y-noffset);
if ( i.texcoord.x > _ClipRight)  c1*= saturate(((1+noffset)/(1-_ClipRight))*(1-i.texcoord.x)-noffset);
if ( i.texcoord.x < 1-_ClipLeft) c1*= saturate(((1+noffset)/(1-_ClipLeft))*i.texcoord.x-noffset);
  mainColor.a = (alpha*c1)-_Alpha;
  return float4(mainColor.rgb,mainColor.a);
}

ENDCG
}
}
Fallback "Sprites/Default"
}