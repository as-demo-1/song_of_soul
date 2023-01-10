//////////////////////////////////////////////
/// 2DxFX v3 - by VETASOFT 2018 //
//////////////////////////////////////////////


//////////////////////////////////////////////

Shader "2DxFX_Extra_Shaders/MotionBlurFast"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
_MotionBlurFast_Angle_1("_MotionBlurFast_Angle_1", Range(-1, 1)) = 0
_MotionBlurFast_Distance_1("_MotionBlurFast_Distance_1", Range(0, 16)) = 1
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
float _MotionBlurFast_Angle_1;
float _MotionBlurFast_Distance_1;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}


float4 MotionBlurFast(sampler2D txt, float2 uv, float angle, float dist)
{
angle = angle *3.1415926;
#define rot(n) mul(n, float2x2(cos(angle), -sin(angle), sin(angle), cos(angle)))
float4 r = float4(0, 0, 0, 0);
dist = dist * 0.005;
r += tex2D(txt, uv + rot(float2(-dist, -dist)));
r += tex2D(txt, uv + rot(float2(-dist*2, -dist*2)));
r += tex2D(txt, uv + rot(float2(-dist*3, -dist*3)));
r += tex2D(txt, uv + rot(float2(-dist*4, -dist*4)));
r += tex2D(txt, uv);
r += tex2D(txt, uv + rot(float2( dist, dist)));
r += tex2D(txt, uv + rot(float2( dist*2, dist*2)));
r += tex2D(txt, uv + rot(float2( dist*3, dist*3)));
r += tex2D(txt, uv + rot(float2( dist*4, dist*4)));
r = r/9;
return r;
}
float4 frag (v2f i) : COLOR
{
float4 _MotionBlurFast_1 = MotionBlurFast(_MainTex,i.texcoord,_MotionBlurFast_Angle_1,_MotionBlurFast_Distance_1);
float4 FinalResult = _MotionBlurFast_1;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
