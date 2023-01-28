// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/GrassMultiFX"
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_Color ("_Color", Color) = (1,1,1,1)
_Distortion ("Distortion", Range(0,1)) = 0
_Wind ("_Wind", Range(0.0, 10.0)) = 1
_Wind2 ("_Wind2", Range(0.0, 10.0)) = 1
_Wind3 ("_Wind2", Range(0.0, 10.0)) = 1
_Wind4 ("_Wind2", Range(0.0, 10.0)) = 1
_Alpha ("Alpha", Range (0,1)) = 1.0
_Speed ("Speed", Range (0,1)) = 1.0
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
float _Wind;
float _Wind2;
float _Wind3;
float _Wind4;
float _Alpha;
float _Speed;
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
	
float2 uv = i.texcoord;
float2 uv2 = i.texcoord+float2(0.2,0.01);
float2 uv3 = i.texcoord+float2(0.4,0.02);
float2 uv4 = i.texcoord+float2(0.6,0.03);
float time=_Time*8*_Speed;

float sn=uv.x+_Wind;
float sy=uv.y/_Distortion;
uv.x=abs(lerp(uv.x,sn,sy));
uv.x=fmod(uv.x,1);
sn=uv2.x+_Wind2;
sy=uv2.y/_Distortion;
uv2.x=abs(lerp(uv2.x,sn,sy));
uv2.x=fmod(uv2.x,1);
sn=uv3.x+_Wind3;
sy=uv3.y/_Distortion;
uv3.x=abs(lerp(uv3.x,sn,sy));
uv3.x=fmod(uv3.x,1);
sn=uv4.x+_Wind4;
sy=uv4.y/_Distortion;
uv4.x=abs(lerp(uv4.x,sn,sy));
uv4.x=fmod(uv4.x,1);


float4 r1=tex2D(_MainTex, uv)*i.color;
float4 r2=tex2D(_MainTex, uv2)*i.color;
float4 r3=tex2D(_MainTex, uv3)*i.color;
float4 r4=tex2D(_MainTex, uv4)*i.color;
r1.a-=0.01;
r2.a-=0.01;
r3.a-=0.01;
r4.a-=0.01;
r1.rgb-=0.195;
r2.rgb-=0.130;
r3.rgb-=0.065;
r4.rgb-=0;


float4 mo=r1 * (1.0 - (r2.a));
float4 bo=r2 * r2.a;
r1=mo+bo;

mo=r1 * (1.0 - (r3.a));
bo=r3 * r3.a;
r1=mo+bo;

mo=r1 * (1.0 - (r4.a));
bo=r4 * r4.a;
r1=mo+bo;

r1.a = r1.a*1-_Alpha;
return r1;

}

ENDCG
}
}
Fallback "Sprites/Default"

}