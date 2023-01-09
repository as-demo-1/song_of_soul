//////////////////////////////////////////////
/// 2DxFX v3 - by VETASOFT 2018 //
//////////////////////////////////////////////


//////////////////////////////////////////////

Shader "2DxFX_Extra_Shaders/RoarDistortion"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
PosX("PosX", Range(0, 1)) = 0.5
PosY("PosY", Range(0, 1)) = 0.5
Size("Size", Range(0, 2)) = 1
LineSize("LineSize", Range(-16, 16)) = 1
Speed("Speed", Range(-2, 2)) = 0
Roar_Intensity("Roar_Intensity", Range(-0.3, 0.3)) = 0.022
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
float PosX;
float PosY;
float Size;
float LineSize;
float Speed;
float Roar_Intensity;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}


float4 Generate_Spiral(float2 uv, float posX, float posY, float Size, float LineSize, float Speed,float black)
{
float t = _Time*Speed*8;
float2 m = float2(posX, posY) - uv;
float r = length(m)*Size;
float a = atan2(m.y, m.x);
float v = sin(100.* (sqrt(r) - (0.02*LineSize) * a - .3 * t));
float4 result = clamp(v, 0., 1.);
result.a = saturate(result.a + black);
return result;
}
float2 SimpleDisplacementUV(float2 uv,float x, float y, float value)
{
return lerp(uv,uv+float2(x,y),value);
}
float4 frag (v2f i) : COLOR
{
float4 _Generate_Spiral_1 = Generate_Spiral(i.texcoord,PosX,PosY,Size,LineSize,Speed,0);
float2 _Simple_Displacement_1 = SimpleDisplacementUV(i.texcoord,_Generate_Spiral_1.r*_Generate_Spiral_1.a,0,Roar_Intensity);
float4 _MainTex_1 = tex2D(_MainTex,_Simple_Displacement_1);
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
