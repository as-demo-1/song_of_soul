//////////////////////////////////////////////
/// 2DxFX v3 - by VETASOFT 2018 //
//////////////////////////////////////////////


//////////////////////////////////////////////

Shader "2DxFX_Extra_Shaders/Emboss"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
_Emboss_Angle_1("_Emboss_Angle_1", Range(-1, 1)) = 0
_Emboss_Distance_1("_Emboss_Distance_1", Range(0, 16)) = 0.3
_Emboss_Intensity_1("_Emboss_Intensity_1", Range(-2, 2)) = 1.479
_Emboss_grayfade_1("_Emboss_grayfade_1", Range(-2, 2)) = 1
_Emboss_original_1("_Emboss_original_1", Range(-2, 2)) = 1
_SpriteFade("SpriteFade", Range(0, 1)) = 1.0

// required for UI.Mask
[HideInInspector]_StencilComp("Stencil Comparison", Float) = 8
[HideInInspector]_Stencil("Stencil ID", Float) = 0
[HideInInspector]_StencilOp("Stencil Operation", Float) = 0
[HideInInspector]_StencilWriteMask("Stencil Write Mask", Float) = 255
[HideInInspector]_StencilReadMask("Stencil Read Mask", Float) = 255
[HideInInspector]_ColorMask("Color Mask", Float) = 15

}

SubShader
{

Tags {"Queue" = "Transparent" "IgnoreProjector" = "true" "RenderType" = "Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
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

struct appdata_t{
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
float _SpriteFade;
float _Emboss_Angle_1;
float _Emboss_Distance_1;
float _Emboss_Intensity_1;
float _Emboss_grayfade_1;
float _Emboss_original_1;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}


float4 Emboss(sampler2D txt, float2 uv, float angle, float dist, float intensity, float g, float o)
{
angle = angle *3.1415926;
intensity = intensity *0.25;
#define rot(n) mul(n, float2x2(cos(angle), -sin(angle), sin(angle), cos(angle)))
float m1 = 1; float m2 = 0; float m3 = 0;
float m4 = 0; float m5 = 0; float m6 = 0;
float m7 = 0; float m8 = 0; float m9 = -1;
float Offset = 0.5;
float Scale = 1;
float4 r = float4(0, 0, 0, 0);
dist = dist * 0.005;
float4 rgb = tex2D(txt, uv);
r += tex2D(txt, uv + rot(float2(-dist, -dist))).a * m1*intensity;
r += tex2D(txt, uv + rot(float2(0, -dist))).a * m2*intensity;
r += tex2D(txt, uv + rot(float2(dist, -dist))).a * m3*intensity;
r += tex2D(txt, uv + rot(float2(-dist, 0))).a * m4*intensity;
r += tex2D(txt, uv + rot(float2(0, 0))).a* m5*intensity;
r += tex2D(txt, uv + rot(float2(dist, 0))).a * m6*intensity;
r += tex2D(txt, uv + rot(float2(-dist, dist))).a * m7*intensity;
r += tex2D(txt, uv + rot(float2(0, dist))).a * m8*intensity;
r += tex2D(txt, uv + rot(float2(dist, dist))).a * m9*intensity;
r = lerp(r,dot(r.rgb,3),g);
r = lerp(r+0.5,rgb+r,o);
r = saturate(r);
r.a = rgb.a;
return r;
}
float4 frag (v2f i) : COLOR
{
float4 _Emboss_1 = Emboss(_MainTex,i.texcoord,_Emboss_Angle_1,_Emboss_Distance_1,_Emboss_Intensity_1,_Emboss_grayfade_1,_Emboss_original_1);
float4 FinalResult = _Emboss_1;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
