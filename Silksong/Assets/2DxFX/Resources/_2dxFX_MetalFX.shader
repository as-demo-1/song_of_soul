// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/MetalFX"
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_Color ("_Color", Color) = (1,1,1,1)
_Distortion ("Distortion", Range(0,1)) = 0
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

float3 Metal(float _t)
{
   
    float u = (0.860117757 + 1.54118254e-4*_t + 1.28641212e-7*_t*_t) / (1.0 + 8.42420235e-4*_t + 7.08145163e-7*_t*_t);
    float v = (0.317398726 + 4.22806245e-5*_t + 4.20481691e-8*_t*_t) / (1.0 - 2.89741816e-5*_t + 1.61456053e-7*_t*_t);
    float x = 3.0 * u / (2.0 * u - 8.0 * v + 4.0);
    float y = 2.0 * v / (2.0 * u - 8.0 * v + 4.0);
    float z = 1.0 - x - y;
    float Y = 1.0;
    float X = (Y/y) * x;
    float Z = (Y/y) * z;
    float3 RGB = float3(Z,Y,X)/_Distortion; 
    RGB.x = RGB.x * pow(0.0006*_t, 4.0);
    RGB.y = RGB.y * pow(0.0004*_t, 4.0);
    RGB.z = RGB.z * pow(0.0004*_t, 4.0);
    return RGB;
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
	uv *= 2.5;
	float a = 1.1 + _Time/4 *20 * 2.25;
	float n = sin(a + 2.0 * uv.x) + sin(a - 2.0 * uv.x) + sin(a + 2.0 * uv.y) + sin(a + 5.0 * uv.y);
	n = mod(((5.0 + n) / 5.0), 1.0);
	n += tex2D(_MainTex, tuv).r * 0.2 + tex2D(_MainTex, tuv).g * 0.4 + tex2D(_MainTex, tuv).b * 0.2;
	return rainbow(n);
}

	

float4 frag (v2f i) : COLOR
{
	float2 uv =  i.texcoord.xy;
    float4 noise = tex2D(_MainTex, uv)*i.color;
    float lum = dot(noise.rgb, float3 (0.4126, 0.8152, 0.1722));
    float maxTemp = 4000.0;
    float tempScale = maxTemp;
    float3 c = Metal(lum * tempScale);
	float alpha = tex2D(_MainTex, i.texcoord).a;
	float4 sortie=plasma(i.texcoord);
	sortie.a=sortie.a*alpha-_Alpha;
	sortie.rgb=c.rgb+(1-sortie.a);
	sortie.rgb=0.1+sortie.rgb/2+dot(sortie.rgb, float3 (0.2126, 0.2152, 0.1722));
	return float4(sortie.rgb,noise.a*1-_Alpha);	
}

ENDCG
}
}
Fallback "Sprites/Default"

}