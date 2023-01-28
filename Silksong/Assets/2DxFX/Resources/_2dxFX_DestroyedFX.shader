// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/DestroyedFX"
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_Color ("_Color", Color) = (1,1,1,1)
_Distortion ("Distortion", Range(0,1)) = 0
_Size ("Size", Range(0,1)) = 0
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
#pragma target 3.0
#include "UnityCG.cginc"

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
float4 _Color;
float _Distortion;
float _Alpha;
float _Size;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;

return OUT;
}

float r (float2 c){
return frac(43.*sin(c.x+7.*c.y)*_Size);
}

float n (float2 p){
float2 i = floor(p), w = p-i, j = float2 (1.,0.);
w = w*w*(3.-w-w);
return lerp(lerp(r(i), r(i+j), w.x), lerp(r(i+j.yx), r(i+1.), w.x), w.y);
}

float a (float2 p){
float m = 0., f = 2.;
for ( int i=0; i<9; i++ ){ m += n(f*p)/f; f+=f; }
return m;
}

float4 frag (v2f i) : COLOR
{
float2 uv = i.texcoord;
float4 tex = tex2D(_MainTex, uv)*i.color;
float t = frac(_Distortion*0.9999);
float4 c = smoothstep(t/1.2, t+.1, a(3.5*uv));
c=tex*c;
c.r=lerp(c.r,c.r*15.0*(1-c.a)*8,_Distortion);
c.g=lerp(c.g,c.g*10.0*(1-c.a)*4,_Distortion);
c.b=lerp(c.b,c.b*5.0*(1-c.a),_Distortion);

return float4(c.rgb,c.a*1-_Alpha);



}

ENDCG
}
}
Fallback "Sprites/Default"

}