// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/Frozen"
{
Properties
{
[HideInInspector] _MainTex ("Base (RGB)", 2D) = "white" {}
[HideInInspector] _MainTex2 ("Pattern (RGB)", 2D) = "white" {}
[HideInInspector] _Alpha ("Alpha", Range (0,1)) = 1.0
[HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
[HideInInspector] _Value1 ("_Value1", Range (0,1)) = 0
[HideInInspector] _Value2 ("_Value2", Range (0,1)) = 0
[HideInInspector] _Value3 ("_Value3", Range (0,1)) = 0
[HideInInspector] _Value4 ("_Value4", Range (0,1)) = 0
[HideInInspector] _Value5 ("_Value5", Range (0,1)) = 0
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

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;

return OUT;
}

sampler2D _MainTex;
sampler2D _MainTex2;
float4 _Color;
float _Alpha;
float _Value1;
float _Value2;
float _Value3;
float _Value4;
float _Value5;

float hardLight( float s, float d )
{
	return (s < 0.5) ? 2.0 * s * d : 1.0 - 2.0 * (1.0 - s) * (1.0 - d);
}

float3 hardLight( float3 s, float3 d )
{
	float3 c=float3(0,0,0);
	c.x = hardLight(s.x,d.x);
	c.y = hardLight(s.y,d.y);
	c.z = hardLight(s.z,d.z);
	return c;
}

float4 frag(v2f IN) : COLOR
{

float speed=_Time*2;
float2 uv=IN.texcoord;
uv=uv*(1-(_Value2*0.4))+float2(_Value2*0.2,_Value2*0.2);
float4 t2 =  tex2D(_MainTex2, uv);
uv=uv*(1-(_Value2*0.4))+float2(_Value2*0.2,_Value2*0.2);
float4 t3 =  tex2D(_MainTex2, uv)*2;
uv=IN.texcoord+float2(-_Value2-speed,0);
float4 t4 =  tex2D(_MainTex2, uv+float2(t2.r,t2.r))*0.5;
float tx=t3.b;
tx=lerp(0,t3.b*0.015,_Value2);
float4 td =  tex2D(_MainTex, IN.texcoord)*IN.color;
float4 t =  tex2D(_MainTex, IN.texcoord+float2(tx,tx))*IN.color;
t2.a = t.a;
t2.rgb = float3(t2.r/10,t2.r/1.5,t2.r);
float g = (t.r+t.g+t.b)/3;
float3 r = smoothstep(_Value1,_Value1+0.1,g)+.2;
t.rgb=lerp(t.rgb,hardLight(g,t2.rgb)+t2.b*r*t3.b+t4.g,_Value2);
return float4(t.rgb,t.a*(1-_Alpha));
}
ENDCG
}
}
Fallback "Sprites/Default"

}