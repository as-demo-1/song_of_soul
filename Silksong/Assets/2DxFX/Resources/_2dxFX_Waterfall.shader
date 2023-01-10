// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/Waterfall"
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
[HideInInspector] _MainTex2 ("Pattern (RGB)", 2D) = "white" {}
_Color ("_Color", Color) = (1,1,1,1)
Lightcolor ("Lightcolor", Color) = (1,1,1,1)
_Distortion ("Distortion", Range(0,1)) = 0
_Alpha ("Alpha", Range (0,1)) = 1.0
_Speed ("Speed", Range (0,1)) = 1.0
EValue ("EValue", Range (0,1)) = 1.0
TValue ("TValue", Range (0,1)) = 1.0
Light ("Light", Range (0,1)) = 1.0

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

GrabPass { "_GrabTexture"  }

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
float2 screenuv : TEXCOORD1;
float4 color    : COLOR;
};



sampler2D _GrabTexture;
sampler2D _MainTex;
sampler2D _MainTex2;

float _Distortion;
float _Alpha;
float _Speed;
float EValue;
float TValue;
float4 _Color;
float4 Lightcolor;
float Light;
uniform float2 _MainTex_TexelSize;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);

float4 screenpos = ComputeGrabScreenPos(OUT.vertex);
OUT.screenuv = screenpos.xy / screenpos.w;

OUT.texcoord = IN.texcoord;
OUT.color = IN.color;

return OUT;
}

float4 frag (v2f i) : COLOR
{

float2 p = i.texcoord.xy;
#if UNITY_UV_STARTS_AT_TOP
if (_MainTex_TexelSize.y < 0)
p.y = 1-p.y;
#endif
p = i.screenuv.xy;
float2 p2 = p;
float4 txt = tex2D(_MainTex,i.texcoord.xy);
float2 uv=i.texcoord.xy;
uv.y+=_Time*6*_Speed+txt.a*txt.r;
float txt2 = tex2D(_MainTex2,uv).r;
uv.y+=_Time*6.*_Speed+txt.a*txt.r;
txt2 += tex2D(_MainTex2,uv).g;
uv.y+=_Time*8.*_Speed+txt.a*txt.r;
txt2 += tex2D(_MainTex2,uv).b;
txt2 /=3.;
float d=txt2.r/64;
d*=_Distortion;
p-=d;
uv=i.texcoord.xy;
uv/=6;
uv.y+=_Time*4*_Speed;
uv.x+=_Time*_Speed;
txt2 += tex2D(_MainTex2,uv).r;
d=txt2/64;
p-=d;
p=lerp(p2,p,_Alpha);
float4 lc=lerp(float4(0,0,0,0),Lightcolor,Lightcolor.a);
float4 gtxt = tex2D(_GrabTexture,p)*i.color+lc;
float3 glight= float3(txt2,txt2,txt2)*Light*Lightcolor;
glight+=tex2D(_MainTex,i.texcoord.xy)*Light*Lightcolor;
glight+=(clamp(txt2,0.6,1)-0.6);
gtxt.rgb+=glight*EValue;
gtxt.rgb+=txt.r*TValue;

return float4(gtxt.rgb,txt.a*_Alpha);
}

ENDCG
}
}
Fallback "Sprites/Default"

}