// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/WaterAndBackgroundDeluxe"
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_Color ("_Color", Color) = (1,1,1,1)
_Distortion ("Distortion", Range(0,1)) = 0
_Alpha ("Alpha", Range (0,1)) = 1.0
_Speed ("Speed", Range (0,1)) = 1.0
EValue ("EValue", Range (0,1)) = 1.0
Light ("Light", Range (0,1)) = 1.0

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

GrabPass { "_GrabTexture"  }

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
float2 screenuv : TEXCOORD1;
float4 color    : COLOR;
};



sampler2D _GrabTexture;
sampler2D _MainTex;
float _Distortion;
float _Alpha;
float _Speed;
float EValue;
float4 _Color;
float Light;
uniform float2 _MainTex_TexelSize;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);

float4 screenpos = ComputeGrabScreenPos(OUT.vertex);
OUT.screenuv = screenpos.xy / screenpos.w;

OUT.texcoord = IN.texcoord;
OUT.color = IN.color;



return OUT;
}
	
float col(float2 coord)
{
float time = _Time*10;
float delta_theta = 0.897597901025655210989326680937;
float col = 0.0;
float theta = 0.0;
for (int i = 0; i < 8; i++)
{
float2 adjc = coord;
theta = delta_theta*float(i);
adjc.x += cos(theta)*time*_Speed + time * _Distortion/4;
adjc.y -= sin(theta)*time*_Speed - time * _Distortion/4;
col = col + cos( (adjc.x*cos(theta) - adjc.y*sin(theta))*6.0);
}

return cos(col);
}

float4 frag (v2f i) : COLOR
{
float2 p = i.texcoord.xy;
#if UNITY_UV_STARTS_AT_TOP
if (_MainTex_TexelSize.y < 0)
p.y = 1-p.y;
#endif
p = i.screenuv.xy;

float2 c1 = p, c2 = p;
float4 col2 = tex2D(_MainTex,i.texcoord.xy);
float colr = col2.r;
col2*=i.color;
float cc1 = col(c1);
c2.x += 8.53;
float dx =  0.50*(cc1-col(c2))/128;
c2.x = p.x;
c2.y += 8.53;
float dy =  0.50*(cc1-col(c2))/128;
c1.x += dx*2.;
c1.y = (c1.y+dy*2.);
float alpha = 1.+dot(dx,dy)*5000*Light;
float ddx = dx - 0.012;
float ddy = dy - 0.012;
if (ddx > 0. && ddy > 0.) alpha = pow(alpha, ddx*ddy*200000);

c1=lerp(p,c1,EValue*col2.a);
c1.y-=(colr/4)*2;
c1.y+=0.1;
float4 col = tex2D(_GrabTexture,c1)*alpha;
col.rgb+=col2.rgb*i.color.rgb*2;
col.a*=col2.a;
return float4(col.rgb,col.a*(1-_Alpha));
}

ENDCG
}
}
Fallback "Sprites/Default"

}