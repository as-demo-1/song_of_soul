//////////////////////////////////////////////
/// 2DxFX v3 - by VETASOFT 2018 //
//////////////////////////////////////////////


//////////////////////////////////////////////

Shader "2DxFX_Extra_Shaders/InsideDistortion"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
Inside_Distance("Inside_Distance", Range(-0.3, 0.3)) = 0.3
Inside_Rotation("Inside_Rotation", Range(-1, 1)) = -0.099
Inside_Fade("Inside_Fade", Range(0, 1)) = 1
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
float Inside_Distance;
float Inside_Rotation;
float Inside_Fade;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}



float4 grayscale(float4 txt,float fade)
{
float3 gs = dot(txt.rgb, float3(0.3, 0.59, 0.11));
return lerp(txt,float4(gs, txt.a), fade);
}

float2 SimpleDisplacementRotativeUV(float2 uv, float4 rgba, float value, float value2)
{
float angle = value2 * 3.1415926;
float dist = rgba.r;
#define rot(n) mul(n, float2x2(cos(angle), -sin(angle), sin(angle), cos(angle)))
float2 uv2 = uv+rot(float2(dist-0.5, dist-0.5));
return lerp(uv, uv2, value);
}
float4 frag (v2f i) : COLOR
{
float4 SourceRGBA_2 = tex2D(_MainTex, i.texcoord);
float4 GrayScale_1 = grayscale(SourceRGBA_2,1);
float2 _Simple_Displacement_Rotative_1 = SimpleDisplacementRotativeUV(i.texcoord,GrayScale_1,Inside_Distance,Inside_Rotation);
i.texcoord = lerp(i.texcoord,_Simple_Displacement_Rotative_1,Inside_Fade);
float4 SourceRGBA_1 = tex2D(_MainTex, i.texcoord);
SourceRGBA_1.a = lerp(SourceRGBA_2.a * SourceRGBA_1.a, (1 - SourceRGBA_2.a) * SourceRGBA_1.a,0);
float4 FinalResult = SourceRGBA_1;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
