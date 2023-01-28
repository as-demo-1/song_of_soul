// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/Blur" 
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_Color ("_Color", Color) = (1,1,1,1)
_Distortion ("Distortion", Range(0,1)) = 0
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

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}

float4 frag (v2f i) : COLOR
{
float stepU = 0.00390625f * _Distortion;
float stepV = stepU;

				
float4 result = float4 (0,0,0,0);
float4 Alpha = tex2D(_MainTex, i.texcoord);
			
float2 texCoord=float2(0,0);

texCoord = i.texcoord.xy + float2( -stepU, -stepV ); result += tex2D(_MainTex,texCoord);
texCoord = i.texcoord.xy + float2( -stepU, 0 ); result += 2.0 * tex2D(_MainTex,texCoord);
texCoord = i.texcoord.xy + float2( -stepU, stepV ); result += tex2D(_MainTex,texCoord);
texCoord = i.texcoord.xy + float2( 0, -stepV ); result += 2.0 * tex2D(_MainTex,texCoord);
texCoord = i.texcoord.xy ; result += 4.0 * tex2D(_MainTex,texCoord);
texCoord = i.texcoord.xy + float2( 0, stepV ); result += 2.0 * tex2D(_MainTex,texCoord);
texCoord = i.texcoord.xy + float2( stepU, -stepV ); result += tex2D(_MainTex,texCoord);
texCoord = i.texcoord.xy + float2( stepU, 0 ); result += 2.0* tex2D(_MainTex,texCoord);
texCoord = i.texcoord.xy + float2( stepU, -stepV ); result += tex2D(_MainTex,texCoord);

float4 r=float4(0,0,0,0);
r=result*0.0625;
r.a*=Alpha.a*(1.0-_Alpha);
r=r*i.color;		
return r;	
				
}

ENDCG
}
}
Fallback "Sprites/Default"

}