// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/BlackHole" 
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_Color ("_Color", Color) = (1,1,1,1)
_Size ("Size", Range(0,1)) = 0
_Distortion ("Distortion", Range(0,1)) = 0
_Hole ("Hole", Range(0,1)) = 0
_Speed ("Speed", Range(0,1)) = 0
_Alpha ("Alpha", Range (0,1)) = 1.0
_Color ("ColorX", Color) = (1,1,1,1)

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
float4 _ColorX;
float _Size;
float _Distortion;
float _Hole;
float _Speed;
float _Alpha;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;



return OUT;
}



float4 hole(sampler2D tex, float2 uv, float time)
{
	float radius = 0.5;
	float2 center = float2(0.5,0.5);
	float2 tc = uv - center;
	float dist = length(tc);
	if (dist < radius)
	{
		float percent = (radius - dist) / radius;
		float theta = percent * percent * (2.0 * sin(time)) * 8.0;
		float s = sin(theta);
		float c = cos(theta);
		tc = float2(dot(tc, float2(c, -s)), dot(tc, float2(s, c)));
	}
	tc += center;
	float4 color = tex2D(tex, tc);
	return color;
}


float4 frag (v2f i) : COLOR
{
_Speed*=5.0f;
float2 uv = (i.texcoord-float2(0.5,0.5))*1.246;
float sinX = sin ( _Speed * _Time );
float cosX = cos ( _Speed * _Time );
float sinY = sinX;
float2x2 rotationMatrix = float2x2( cosX, -sinX, sinY, cosX);
uv.xy = mul (  uv, rotationMatrix )+float2(0.5,0.5);
float dist = 1.0 - smoothstep( _Hole,_Hole+0.15, length(float2(0.5,0.5)-uv) );
float dista = 1.0 - smoothstep( 0.25,0.5, length(float2(0.5,0.5)-uv) );
float4 finalColor = hole(_MainTex, uv, _Distortion);
finalColor.rgb*=1-dist;
finalColor.a=finalColor.a*(1-_Alpha);
finalColor.a*=dista*(1-dist);
return finalColor*i.color*_ColorX;

}

ENDCG
}
}
Fallback "Sprites/Default"

}