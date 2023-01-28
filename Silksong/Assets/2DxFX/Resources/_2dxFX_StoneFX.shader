// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/StoneFX"
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_Distortion ("Distortion", Range(0,1)) = 0
_Alpha ("Alpha", Range (0,1)) = 1.0
_Deep ("Alpha", Range (0,1)) = 1.0
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
float _Deep;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;

return OUT;
} 


inline float mod(float x,float modu) 
{
return x - floor(x * (1.0 / modu)) * modu;
}   

float4 rainbow(float t) 
{
t=mod(t,1.0);
float tx = t * 6.0;

float r = clamp(tx - 2.0, 0.0, 1.0) + clamp(2.0 - tx, 0.0, 1.0);

return float4(1.0, 1.0, 1.0,r);
}

float4 plasma(float2 uv)
{
float2 tuv = uv;
uv *= 15.0;
float a = 1.1 + 20 * 2.25;
float n = sin(a + 2.0 * uv.x) + sin(a - 2.0 * uv.x) + sin(a + 2.0 * uv.y) + sin(a + 5.0 * uv.y);
n = mod(((5.0 + n) / 5.0), 1.0);
n += tex2D(_MainTex, tuv).r * 11.2 + tex2D(_MainTex, tuv).g * 8.4 + tex2D(_MainTex, tuv).b * 4.2;

return rainbow(n*_Deep);
}



float4 frag (v2f i) : COLOR
{
float2 uv 		=  i.texcoord.xy ;

float4 tex = tex2D(_MainTex, i.texcoord+float2(sin(_Distortion*64)/512,0));
float lum = (tex.r+tex.b+tex.g)/3;
float a=lum;
float rate=0.2;
float r=a*(1-rate);
r=r+(1*(rate/2));
if (r>0.6) r=0.6;
if (r<0.2) r=0.2;
float4 sortie=plasma(i.texcoord);
sortie+=plasma(float2(i.texcoord.y,i.texcoord.x));
float3 r2= r-(1-sortie.a)/8;
tex.rgb=lerp(tex.rgb,r2,_Distortion);   
return float4(tex.rgb,tex.a*1-_Alpha);

}

ENDCG
}
}
Fallback "Sprites/Default"

}