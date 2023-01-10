//////////////////////////////////////////////
/// 2DxFX v3 - by VETASOFT 2018 //
//////////////////////////////////////////////


//////////////////////////////////////////////

Shader "2DxFX_Extra_Shaders/Animated_Zoom"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
AnimatedZoomUV_AnimatedZoomUV_Zoom_1("AnimatedZoomUV_AnimatedZoomUV_Zoom_1", Range(0.2, 4)) = 1
AnimatedZoomUV_AnimatedZoomUV_PosX_1("AnimatedZoomUV_AnimatedZoomUV_PosX_1", Range(-1, 2)) = 0.5
AnimatedZoomUV_AnimatedZoomUV_PosY_1("AnimatedZoomUV_AnimatedZoomUV_PosY_1", Range(-1, 2)) = 0.5
AnimatedZoomUV_AnimatedZoomUV_Intensity_1("AnimatedZoomUV_AnimatedZoomUV_Intensity_1", Range(0, 4)) = 0.5
AnimatedZoomUV_AnimatedZoomUV_Speed_1("AnimatedZoomUV_AnimatedZoomUV_Speed_1", Range(-10, 10)) = 6.393
_LerpUV_Fade_1("_LerpUV_Fade_1", Range(0, 1)) = 1
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
float AnimatedZoomUV_AnimatedZoomUV_Zoom_1;
float AnimatedZoomUV_AnimatedZoomUV_PosX_1;
float AnimatedZoomUV_AnimatedZoomUV_PosY_1;
float AnimatedZoomUV_AnimatedZoomUV_Intensity_1;
float AnimatedZoomUV_AnimatedZoomUV_Speed_1;
float _LerpUV_Fade_1;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}


float2 AnimatedZoomUV(float2 uv, float zoom, float posx, float posy, float radius, float speed)
{
float2 center = float2(posx, posy);
uv -= center;
zoom -= radius * 0.1;
zoom += sin(_Time * speed * 20) * 0.1 * radius;
uv = uv * zoom;
uv += center;
return uv;
}
float4 frag (v2f i) : COLOR
{
float2 AnimatedZoomUV_1 = AnimatedZoomUV(i.texcoord,AnimatedZoomUV_AnimatedZoomUV_Zoom_1,AnimatedZoomUV_AnimatedZoomUV_PosX_1,AnimatedZoomUV_AnimatedZoomUV_PosY_1,AnimatedZoomUV_AnimatedZoomUV_Intensity_1,AnimatedZoomUV_AnimatedZoomUV_Speed_1);
AnimatedZoomUV_1 = saturate(AnimatedZoomUV_1); 
i.texcoord = lerp(i.texcoord,AnimatedZoomUV_1,_LerpUV_Fade_1);
float4 _MainTex_1 = tex2D(_MainTex,i.texcoord);
float4 FinalResult = _MainTex_1;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
