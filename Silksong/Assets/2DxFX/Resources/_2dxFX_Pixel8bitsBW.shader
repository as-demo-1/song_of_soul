// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/Pixel8bitsBW" {
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_Color ("_Color", Color) = (1,1,1,1)
_Size ("Size", Range(0,1)) = 0
_Offset ("Offset", Range(0,1)) = 0
_Offset2 ("Offset2", Range(0,1)) = 0
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
float _Size;
float _Offset;
float _Offset2;
float _Alpha;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}

float compare(float3 a, float3 b)
{
float3 diff = (a - b);
return dot(diff, diff);
}

inline float mod(float x,float modu)
{
	return x - floor(x * (1.0 / modu)) * modu;
}

float4 frag (v2f i) : COLOR
{

float2 q  = i.texcoord;
float2 pixelSize=float2(64*_Size,64*_Size);
float2 c = floor(q * pixelSize)/pixelSize;
float4 src4 = tex2D(_MainTex, c);
float3 src = src4.rgb*_Offset2;
float alpha = src4.a;
if (alpha<0.95) alpha=0;
float3 dst0 = float3(0,0,0);
float3 dst1 = float3(0,0,0);
float best0 = 1e3;
float best1 = 1e3;
#define TCOLOR(R, G, B) { const float3 tst = float3(R, G, B); float err = compare(src, tst); if (err < best0) { best1 = best0; dst1 = dst0; best0 = err; dst0 = tst; } }
TCOLOR(0.98, 0.98, 0.98);
TCOLOR(0.03, 0.03, 0.03);
#undef TCOLOR

best0 = sqrt(best0); best1 = sqrt(best1);
float4 FragColor = float4(mod(c.x + c.y, 2.0) > (1+best1 / (best0 + best1)) ? dst1 : dst0, 1.0);
FragColor.a=FragColor.a*alpha*(1-_Alpha);
return FragColor*i.color;


}
ENDCG
}
}
Fallback "Sprites/Default"

}