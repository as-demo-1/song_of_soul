//////////////////////////////////////////////
/// 2DxFX v3 - by VETASOFT 2018 //
//////////////////////////////////////////////


//////////////////////////////////////////////

Shader "2DxFX_Extra_Shaders/Perspective3D"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
_Make3DFX_Dist_1("_Make3DFX_Dist_1", Range(-1, 1)) = 0.5
_Make3DFX_Size_1("_Make3DFX_Size_1", Range(1, 16)) = 16
_Make3DFX_PosX_1("_Make3DFX_PosX_1", Range(-2, 2)) = 0.3
_Make3DFX_PosY_1("_Make3DFX_PosY_1", Range(-2, 2)) = 0.2
_Make3DFX_Light_1("_Make3DFX_Light_1", Range(-2, 2)) = 0.2
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
float _Make3DFX_Dist_1;
float _Make3DFX_Size_1;
float _Make3DFX_PosX_1;
float _Make3DFX_PosY_1;
float _Make3DFX_Light_1;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}


float4 Make3DFX(sampler2D smp, float2 uv,float dist, float size, float x, float y, float light)
{
float4 overlay = float4(0, 0, 0, 1);
float4 origin = tex2D(smp, uv);
dist *= 0.03;
for (int i = 0; i < size; i++)
{
uv.x += dist*x;
uv.y += dist*y;
float4 o= float4(0, 0, 0, 1);
overlay = tex2D(smp, uv);
float z = i / size;
origin.rgb =  origin.rgb = lerp(origin.rgb +(light/size)*2, origin.rgb, z);
origin = saturate(origin);
o.a = overlay.a + origin.a * (1 - overlay.a);
o.rgb = (overlay.rgb * overlay.a + origin.rgb * origin.a * (1 - overlay.a)) / (o.a+0.0000001);
o.a = saturate(o.a);
o = lerp(origin, o, 1);
origin = o;
}
return origin;
}
float4 frag (v2f i) : COLOR
{
float4 _Make3DFX_1 = Make3DFX(_MainTex,i.texcoord,_Make3DFX_Dist_1,_Make3DFX_Size_1,_Make3DFX_PosX_1,_Make3DFX_PosY_1,_Make3DFX_Light_1);
float4 FinalResult = _Make3DFX_1;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
