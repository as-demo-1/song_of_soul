// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/HSV"
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_Alpha ("Alpha", Range (0,1)) = 1.0
_Color ("_Color", Color) = (1,1,1,1)
_HueShift("HueShift",  Range (0,360) ) = 0

_Sat("Saturation", Float) = 1
_Val("Value", Float) = 1
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
float _HueShift;
float _Sat;
float _Val;
float _Alpha;
float4 _Color;

float3 shift_col(float3 RGB, float3 shift)
{

float3 RESULT = float3(RGB);
float a1= shift.z*shift.y;
float a2= shift.x*3.14159265/180;
float VSU = a1*cos(a2);
float VSW = a1*sin(a2);

RESULT.x = (.299*shift.z+.701*VSU+.168*VSW)*RGB.x
+ (.587*shift.z-.587*VSU+.330*VSW)*RGB.y
+ (.114*shift.z-.114*VSU-.497*VSW)*RGB.z;

RESULT.y = (.299*shift.z-.299*VSU-.328*VSW)*RGB.x
+ (.587*shift.z+.413*VSU+.035*VSW)*RGB.y
+ (.114*shift.z-.114*VSU+.292*VSW)*RGB.z;

RESULT.z = (.299*shift.z-.3*VSU+1.25*VSW)*RGB.x
+ (.587*shift.z-.588*VSU-1.05*VSW)*RGB.y
+ (.114*shift.z+.886*VSU-.203*VSW)*RGB.z;

return (RESULT);
}


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
float4 c = tex2D(_MainTex, i.texcoord)*i.color;

float3 shift = float3(_HueShift, _Sat, _Val);
c.rgb = shift_col(c, shift);
c.a = c.a-_Alpha;

return c;
}
ENDCG
}
}
Fallback "Sprites/Default"

}