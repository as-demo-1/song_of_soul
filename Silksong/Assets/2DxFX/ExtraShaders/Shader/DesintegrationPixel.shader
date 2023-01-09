//////////////////////////////////////////////
/// 2DxFX v3 - by VETASOFT 2018 //
//////////////////////////////////////////////


//////////////////////////////////////////////

Shader "2DxFX_Extra_Shaders/DesintegrationPixel"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
Desintegration_Pixel_Size("Desintegration_Pixel_Size", Range(1, 128)) = 19
Desintegration_Value("Desintegration_Value", Range(0, 1)) = 0
Desintegration_Speed("Desintegration_Speed", Range(0, 1)) = 0.5
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
float Desintegration_Pixel_Size;
float Desintegration_Value;
float Desintegration_Speed;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}


float DGFXr (float2 c, float seed)
{
return frac(43.*sin(c.x+7.*c.y)*seed);
}

float DGFXn (float2 p, float seed)
{
float2 i = floor(p), w = p-i, j = float2 (1.,0.);
w = w*w*(3.-w-w);
return lerp(lerp(DGFXr(i, seed), DGFXr(i+j, seed), w.x), lerp(DGFXr(i+j.yx, seed), DGFXr(i+1., seed), w.x), w.y);
}

float DGFXa (float2 p, float seed)
{
float m = 0., f = 2.;
for ( int i=0; i<9; i++ ){ m += DGFXn(f*p, seed)/f; f+=f; }
return m;
}

float4 DesintegrationFX(float4 txt, float2 uv, float value, float seed)
{

float t = frac(value*0.9999);
float4 c = smoothstep(t / 1.2, t + .1, DGFXa(3.5*uv, seed));
c = txt*c;
c.r = lerp(c.r, c.r*(1 - c.a), value);
c.g = lerp(c.g, c.g*(1 - c.a), value);
c.b = lerp(c.b, c.b*(1 - c.a), value);
return c;
}
float BFXr (float2 c, float seed)
{
return frac(43.*sin(c.x+7.*c.y)* seed);
}

float BFXn (float2 p, float seed)
{
float2 i = floor(p), w = p-i, j = float2 (1.,0.);
w = w*w*(3.-w-w);
return lerp(lerp(BFXr(i, seed), BFXr(i+j, seed), w.x), lerp(BFXr(i+j.yx, seed), BFXr(i+1., seed), w.x), w.y);
}

float BFXa (float2 p, float seed)
{
float m = 0., f = 2.;
for ( int i=0; i<9; i++ ){ m += BFXn(f*p, seed)/f; f+=f; }
return m;
}

float4 BurnFX(float4 txt, float2 uv, float value, float seed, float HDR)
{
float t = frac(value*0.9999);
float4 c = smoothstep(t / 1.2, t + .1, BFXa(3.5*uv, seed));
c = txt*c;
c.r = lerp(c.r, c.r*15.0*(1 - c.a), value);
c.g = lerp(c.g, c.g*10.0*(1 - c.a), value);
c.b = lerp(c.b, c.b*5.0*(1 - c.a), value);
c.rgb += txt.rgb*value;
c.rgb = lerp(saturate(c.rgb),c.rgb,HDR);
return c;
}
float2 PixelUV(float2 uv, float x)
{
uv = floor(uv * x + 0.5) / x;
return uv;
}
float4 frag (v2f i) : COLOR
{
float2 PixelUV_1 = PixelUV(i.texcoord,Desintegration_Pixel_Size);
float4 _MainTex_1 = tex2D(_MainTex, i.texcoord);
float4 _Desintegration_1 = DesintegrationFX(_MainTex_1,PixelUV_1,Desintegration_Value,Desintegration_Speed);
float4 FinalResult = _Desintegration_1;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
FinalResult.rgb *= FinalResult.a;
FinalResult.a = saturate(FinalResult.a);
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
