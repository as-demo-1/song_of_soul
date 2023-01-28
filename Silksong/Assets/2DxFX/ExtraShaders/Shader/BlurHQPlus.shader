//////////////////////////////////////////////
/// 2DxFX v3 - by VETASOFT 2018 //
//////////////////////////////////////////////


//////////////////////////////////////////////

Shader "2DxFX_Extra_Shaders/BlurHQPlus"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
_BlurHQPlus_Intensity_1("_BlurHQPlus_Intensity_1", Range(1, 64)) = 7.114
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
float _BlurHQPlus_Intensity_1;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}


float BlurHQ_Plus_G(float bhqp, float x)
{
return exp(-(x * x) / (2.0 * bhqp * bhqp));
}

float4 BlurHQ_Plus(float2 uv, sampler2D source, float Intensity)
{
int c_samples = 16;
int c_halfSamples = c_samples / 2;
float c_pixelSize = 0.00390625;
float c_sigmaX = 0.1 + Intensity *0.5;
float c_sigmaY = c_sigmaX;
float total = 0.0;
float4 ret = float4(0, 0, 0, 0);
for (int iy = 0; iy < c_samples; ++iy)
{
float fy = BlurHQ_Plus_G(c_sigmaY, float(iy) - float(c_halfSamples));
float offsety = float(iy - c_halfSamples) * c_pixelSize;
for (int ix = 0; ix < c_samples; ++ix)
{
float fx = BlurHQ_Plus_G(c_sigmaX, float(ix) - float(c_halfSamples));
float offsetx = float(ix - c_halfSamples) * c_pixelSize;
total += fx * fy;
ret += tex2D(source, uv + float2(offsetx, offsety)) * fx*fy;
}
}
return ret / total;
}
float4 frag (v2f i) : COLOR
{
float4 _BlurHQPlus_1 = BlurHQ_Plus(i.texcoord,_MainTex,_BlurHQPlus_Intensity_1);
float4 FinalResult = _BlurHQPlus_1;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
