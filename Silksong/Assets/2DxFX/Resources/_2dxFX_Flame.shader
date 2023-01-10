// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/Flame"
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_MainTex2 ("Base (RGB)", 2D) = "white" {}
_Speed ("_Speed", Range(4,128)) = 4
_Intensity ("_Intensity", Range(4,128)) = 4
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
sampler2D _MainTex2;
float _Speed;
float _Intensity;
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




// -----------------------------------------------
float4 frag (v2f i) : COLOR
{
    
    float2 uv = i.texcoord.xy;
    float2 _uv = uv;
	float _TimeX=_Time*128*_Speed;
	uv -= float2(0.5,0.5);
    float2 centerUV = uv;
    float2 offset = float2(0.0, -_TimeX * 0.15);
    
    // flame thickness
    float2 uv2=i.texcoord+offset;
	float2 uv3=i.texcoord+offset*1.5;
	uv2.y/=16;
	uv3.y/=12;
	
	float flame = 1.3 - length(uv.x) * 3.0;
	float4 t3 =  tex2D(_MainTex2,uv3);
    float4 t2 = tex2D(_MainTex2,uv2);
   
	float variationH = t3.g-t2.g;
  
    uv2.x += i.texcoord.y*cos(_TimeX)/8;
    float4 t =  tex2D(_MainTex, float2(uv2.x,i.texcoord.y));
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
	
	return t2*i.color;
    
}
ENDCG

}
}
Fallback "Sprites/Default"

}
