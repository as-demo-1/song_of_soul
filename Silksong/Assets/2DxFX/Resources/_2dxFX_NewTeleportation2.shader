// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/NewTeleportation2"
{
Properties
{
[HideInInspector] _MainTex ("Base (RGB)", 2D) = "white" {}
[HideInInspector] _MainTex2 ("Pattern (RGB)", 2D) = "white" {}
[HideInInspector] _Alpha ("Alpha", Range (0,1)) = 1.0
[HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
[HideInInspector] _Distortion("_Distortion", Range (0,1)) = 0
[HideInInspector] _Value2 ("_Value2", Range (0,1)) = 0
[HideInInspector] _Value3 ("_Value3", Range (0,1)) = 0
[HideInInspector] _Value4 ("_Value4", Range (0,1)) = 0
[HideInInspector] _Value5 ("_Value5", Range (0,1)) = 0
[HideInInspector] TeleportationColor ("Teleportation Color", Color) = (1,1,1,1)
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

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;

return OUT;
}

sampler2D _MainTex;
sampler2D _MainTex2;
float4 _Color;
float _Alpha;
float _Value1;
float _Distortion;
float4 TeleportationColor;
float _Value2;
float _HDR_Intensity;
float _Fade;

float4 frag(v2f IN) : COLOR
{
float2 uv=IN.texcoord;
float4 txt1 =  tex2D(_MainTex, uv);
float talpha = txt1.a;

float step2 = smoothstep(0.3, 1, _Fade);
float4 txt2 = tex2D(_MainTex2, uv - float2(0, step2*0.4));
float4 txt2r = tex2D(_MainTex2, uv+float2(txt2.r*0.3,txt2.r*0.2));
float4 rtxt = txt2.r;
rtxt.a = talpha;
float3 blue = TeleportationColor.rgb;
float step1 = smoothstep(  0, 0.2, _Fade);
float hdr = lerp(1,_HDR_Intensity, step1);
rtxt.rgb = lerp(txt1.rgb, txt1.rgb+blue, step1);
step1 = smoothstep(0.1, 0.35, _Fade);
rtxt.rgb = lerp(rtxt.rgb, blue *TeleportationColor.rgb + TeleportationColor.rgb, step1);

step1 = smoothstep(0.35, 1.3, _Fade);

float btw = smoothstep(step1, step1 + 0.01, txt2r.b);
float btwa = btw;

float4 txt3 = tex2D(_MainTex2, uv - float2(txt2.r*0.05*_Distortion+uv.x, _Fade))*8;
btwa += txt3.g*2;

step2 = smoothstep(0.12, 1.3, _Fade);
rtxt.rgb = lerp(rtxt.rgb, float3(1.65, 1.65, 1.65), saturate(txt3.g*step2))*hdr;
btw = txt2.r*btw + (1 - step1 * 2);
btw *= btwa*talpha;

step1 = smoothstep(0.1, 0.6, _Fade);
rtxt.a = lerp(rtxt.a, btw, step1);
step1 = smoothstep(0.9, 1, _Fade);
rtxt.a = lerp(rtxt.a, 0, step1);

rtxt.a = saturate(rtxt.a)*(1-_Alpha);
return rtxt;
}

ENDCG
}
}
Fallback "Sprites/Default"

}