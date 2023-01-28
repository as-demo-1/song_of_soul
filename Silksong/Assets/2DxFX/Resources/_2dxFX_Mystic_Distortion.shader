// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/Mystic_Distortion"
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_Pitch ("Pitch", Range(0,0.5)) = 0
_OffsetX ("OffsetX", Range(0,128)) = 0
_OffsetY ("OffsetY", Range(0,128)) = 0
_DistanceX ("DistanceX", Range(0,1)) = 0
_DistanceY ("DistanceY", Range(0,1)) = 0
_WaveTimeX ("WaveTimeX", Range(0,360)) = 0
_WaveTimeY ("WaveTimeY", Range(0,360)) = 0
_Color ("Tint", Color) = (1,1,1,1)
_Alpha ("Alpha", Range (0,1)) = 1.0
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
#pragma target 3.0
			
			
struct appdata_t
{
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
float _OffsetX;
float _OffsetY;
float _Pitch;
float4 _Color;
float _DistanceX;
float _DistanceY;
float _WaveTimeX;
float _WaveTimeY;
float _Alpha;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
float2 p=IN.texcoord;
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}


float4 frag(v2f IN) : COLOR
{
float2 p=IN.texcoord;
p.x= p.x+sin(p.y*_OffsetX+_WaveTimeX)*_DistanceX;
p.y= p.y+cos(p.x*_OffsetY+_WaveTimeY)*_DistanceY;
float2 m = float2(0.5, 0.5);
float2 d = p - m;
float  r = sqrt(dot(d, d)); 
float power = 4.44289334 * (_Pitch - 0.5);
float bind = m.y;
float2 uv=float2(0,0);
uv = m + normalize(d) * atan(r * -power * 10.0) * bind / atan(-power * bind * 10.0);
float4 mainColor = tex2D(_MainTex, uv)* IN.color;
mainColor.a-=_Alpha;
return mainColor;
}
ENDCG
}
}
Fallback "Sprites/Default"

}