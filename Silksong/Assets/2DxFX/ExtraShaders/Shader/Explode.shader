//////////////////////////////////////////////
/// 2DxFX v3 - by VETASOFT 2018 //
//////////////////////////////////////////////


//////////////////////////////////////////////

Shader "2DxFX_Extra_Shaders/Explode"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
Explode_Value("Explode_Value", Range(0, 1)) = 1
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
float Explode_Value;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}


float4 Generate_Noise(float2 co, float black)
{
float4 r = frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
r.a = saturate(r.a + black);
return r;
}
float2 FishEyeUV(float2 uv, float size)
{
float2 m = float2(0.5, 0.5);
float2 d = uv - m;
float r = sqrt(dot(d, d));
float power = (2.0 * 3.141592 / (2.0 * sqrt(dot(m, m)))) * (size+0.001);
float bind = sqrt(dot(m, m));
uv = m + normalize(d) * tan(r * power) * bind / tan(bind * power);
return uv;
}
float2 SimpleDisplacementUV(float2 uv,float x, float y, float value)
{
return lerp(uv,uv+float2(x,y),value);
}
float4 frag (v2f i) : COLOR
{
float2 FishEyeUV_1 = FishEyeUV(i.texcoord,0.482);
float4 _Generate_Line_1 = Generate_Noise(i.texcoord,0);
float2 _Simple_Displacement_1 = SimpleDisplacementUV(FishEyeUV_1,0,_Generate_Line_1.g*_Generate_Line_1.a,-0.3);
i.texcoord = lerp(i.texcoord,_Simple_Displacement_1,Explode_Value);
float4 _MainTex_1 = tex2D(_MainTex, i.texcoord);
float4 FinalResult = _MainTex_1;
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
