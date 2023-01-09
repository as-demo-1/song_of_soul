// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/Hologram3"
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_Size ("Size", Range(0,1)) = 0
_Distortion ("Distortion", Range(0,1)) = 0
_Alpha ("Alpha", Range (0,1)) = 1.0
_Color ("_Color", Color) = (1,1,1,1)
_ColorX ("_ColorX", Color) = (1,1,1,1)
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
ZWrite Off Blend SrcAlpha One Cull Off

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
float4 vertex   : POSITION;
float4 color    : COLOR;
};


sampler2D _MainTex;
float _Size;
float _Distortion;

float _Alpha;
float4 _Color;
float4 _ColorX;
float _TimeX;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;

return OUT;
}


inline float mod(float x,float modu) {
return x - floor(x * (1.0 / modu)) * modu;
}

inline float noise(float2 p)
{
_TimeX=_Time.y;
float sample = tex2D(_MainTex,float2(.2,0.2*cos(_TimeX))*_TimeX*8. + p*1.).x;
sample *= sample;
return sample;
}


inline float onOff(float a, float b, float c)
{
_TimeX=_Time.y;
return step(c, sin(_TimeX + a*cos(_TimeX*b)));
}

inline float ramp(float y, float start, float end)
{
float inside = step(start,y) - step(end,y);
float fact = (y-start)/(end-start)*inside;
return (1.-fact) * inside;

}

inline float stripes(float2 uv)
{
_TimeX=_Time.y;
float noi = noise(uv*float2(0.5,1.) + float2(6.,3.))*_Distortion*3;
return ramp(mod(uv.y*4. + _TimeX/2.+sin(_TimeX + sin(_TimeX*0.63)),1.),.5,.6)*noi;
}

inline float4 getVideo(float2 uv)
{
_TimeX=_Time.y;
float2 look = uv;
float window = 4./(1.+20.*(look.y-mod(_TimeX/1.,1.))*(look.y-mod(_TimeX/10.,1.)));
look.x = look.x + sin(look.y*30. + _TimeX)/(50.*_Distortion)*onOff(1.,4.,.3)*(1.+cos(_TimeX*80.))*window;

float vShift = .4*onOff(2.,3.,.9)*(sin(_TimeX)*sin(_TimeX*200.) +
(0.5 + 0.1*sin(_TimeX*20.)*cos(_TimeX)));

look.y = mod(look.y + vShift, 1.);

float4 video = float4(0,0,0,0);

video.r = tex2D(_MainTex,look-float2(.05,0.)*onOff(2.,1.5,.9)).r;
float4 videox=tex2D(_MainTex,look);
video.g = videox.g;
video.b = tex2D(_MainTex,look+float2(.05,0.)*onOff(2.,1.5,.9)).b;
video.a = videox.a;

return video;
}

inline float2 screenDistort(float2 uv)
{
uv -= float2(.5,.5);
uv = uv*4.2*(1./4.2+2.*uv.x*uv.x*uv.y*uv.y);
uv += float2(.5,.5);
return uv;
}

float4 frag (v2f i) : COLOR
{
float2 uv = i.texcoord.xy;
float alpha = tex2D(_MainTex,uv).a;

uv = screenDistort(uv);
float4 video = getVideo(uv)*i.color;
_TimeX=_Time.y;
float vigAmt = 3.+.3*sin(_TimeX + 5.*cos(_TimeX*5.));
float vignette = (1.-vigAmt*(uv.y-.5)*(uv.y-.5))*(1.-vigAmt*(uv.x-.5)*(uv.x-.5));

video += stripes(uv);
video += noise(uv*2.)/2.;
video.r *= vignette;

video.rgb=video.r*_ColorX;

video *= (12.+mod(uv.y*30.+_TimeX,1.))/13.;
video.a=video.a+(frac(sin(dot(uv.xy*_TimeX,float2(12.9898,78.233))) * 43758.5453))*.5;
video.a=(video.a*.4)*alpha*vignette*4*(1-_Alpha)*i.color.a;

return video;
}

ENDCG
}
}
Fallback "Sprites/Default"

}