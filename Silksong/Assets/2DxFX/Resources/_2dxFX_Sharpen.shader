// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/Sharpen"
{ 
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_Distortion ("Distortion", Range(0,1)) = 0
_Color ("_Color", Color) = (1,1,1,1)
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
	
	
inline float4 sharp(float2 uv)
{
	float r = 1.0/256.0; 
	float strength = 9.0 * _Distortion;

	float4 c0 = tex2D(_MainTex,uv);
	float4 c1 = tex2D(_MainTex,uv-float2(r,.0));
	float4 c2 = tex2D(_MainTex,uv+float2(r,.0));
	float4 c3 = tex2D(_MainTex,uv-float2(.0,r));
	float4 c4 = tex2D(_MainTex,uv+float2(.0,r));
	float cx = c0+c1+c2+c3+c4;
	float4 c5 = float4(cx,cx,cx,cx); c5*=0.2;
	float4 mi = min(c0,c1); mi = min(mi,c2); mi = min(mi,c3); mi = min(mi,c4);
	float4 ma = max(c0,c1); ma = max(ma,c2); ma = max(ma,c3); ma = max(ma,c4);
	float4 rt= clamp(mi,(strength+1.0)*c0-c5*strength,ma);
	return float4(rt.rgb,c0.a);
}

float4 frag (v2f i) : COLOR
{
   	float4 col= sharp(i.texcoord)*i.color;	
	col.a = col.a*1-_Alpha;
	return col;
}

ENDCG
}
}
Fallback "Sprites/Default"

}