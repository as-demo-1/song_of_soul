// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2017 //
/// http://vetasoft.store/2dxfx/            //
//////////////////////////////////////////////

Shader "2DxFX/Standard/BlurHQX" 
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

float Gaussian (float sigma, float x)
{
    return exp(-(x*x) / (2.0 * sigma*sigma));
}

float4 BlurredPixel (in float2 uv)
{
 int   c_samplesX    = 16;  // must be odd
 int   c_samplesY    = 16;  // must be odd
 float c_textureSize = 256.0;
 
 int   c_halfSamplesX = c_samplesX / 2;
 int   c_halfSamplesY = c_samplesY / 2;
 float c_pixelSize = (1.0 / c_textureSize);

    float c_sigmaX      = 0.1+_Distortion*0.5;
	float c_sigmaY      = c_sigmaX;
    
    float total = 0.0;
    float4 ret = float4(0,0,0,0);
        
    for (int iy = 0; iy < c_samplesY; ++iy)
    {
        float fy = Gaussian (c_sigmaY, float(iy) - float(c_halfSamplesY));
        float offsety = float(iy-c_halfSamplesY) * c_pixelSize;
        for (int ix = 0; ix < c_samplesX; ++ix)
        {
            float fx = Gaussian (c_sigmaX, float(ix) - float(c_halfSamplesX));
            float offsetx = float(ix-c_halfSamplesX) * c_pixelSize;
            total += fx * fy;            
            ret += tex2D(_MainTex, uv + float2(offsetx, offsety)) * fx*fy;
        }
    }
    return ret / total;
}
float4 frag (v2f i) : COLOR
{
float2 uv=i.texcoord.xy;
float step1 = 0.00390625f * _Distortion*0.5;
float step2 = step1*2;
						
float4 result = float4 (0,0,0,0);
float4 Alpha = tex2D(_MainTex, uv);
			
float4 r = BlurredPixel(uv);

r.a*=(1-_Alpha);
r=r*i.color;		
return r;	
				
}

ENDCG
}
}
Fallback "Sprites/Default"

}