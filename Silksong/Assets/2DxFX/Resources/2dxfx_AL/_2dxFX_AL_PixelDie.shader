//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/PixelDie"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
[HideInInspector] _MainTex2 ("Pattern (RGB)", 2D) = "white" {}
[HideInInspector] _Alpha ("Alpha", Range (0,1)) = 1.0
[HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
[HideInInspector] _Value1 ("_Value1", Range (0,1)) = 0
[HideInInspector] _Value2 ("_Value2", Range (0,1)) = 0
[HideInInspector] _Value3 ("_Value3", Range (0,1)) = 0
[HideInInspector] _Value4 ("_Value4", Range (0,1)) = 0
[HideInInspector] _Value5 ("_Value5", Range (0,1)) = 0
_Alpha ("Alpha", Range (0,1)) = 1.0
_Color ("Tint", Color) = (1,1,1,1)
[HideInInspector]_SrcBlend("_SrcBlend", Float) = 0
[HideInInspector]_DstBlend("_DstBlend", Float) = 0
[HideInInspector]_BlendOp("_BlendOp",Float) = 0
[HideInInspector]_Z("_Z", Float) = 0
}

SubShader
{
Tags
{
"IgnoreProjector" = "True"
"RenderType" = "TransparentCutout"
"PreviewType" = "Plane"
"CanUseSpriteAtlas" = "True"
}
Cull Off
Lighting Off
ZWrite [_Z]
BlendOp [_BlendOp]
Blend [_SrcBlend] [_DstBlend]

CGPROGRAM
#pragma surface surf Lambert vertex:vert nofog keepalpha addshadow fullforwardshadows

sampler2D _MainTex;
sampler2D _MainTex2;
float4 _Color;
float _Alpha;
float _Value1;
float _Value2;
float _Value3;
float _Value4;
float _Value5;

struct Input
{
float2 uv_MainTex;
float4 color;
};

void vert(inout appdata_full v, out Input o)
{
v.vertex = UnityPixelSnap(v.vertex);
UNITY_INITIALIZE_OUTPUT(Input, o);
o.color = v.color * _Color;
}

void surf(Input IN, inout SurfaceOutput o)
{


float2 uv=IN.uv_MainTex;
float4 t2 = tex2D(_MainTex2, float2(uv.x,uv.y));
t2.rgb= smoothstep(t2.rgb, t2.rgb+0.5, _Value1*1.2);
float r= 1-t2.r;
float4 t = tex2D(_MainTex, float2(uv.x*r,uv.y*r))*IN.color;
t.a*=r;
t.b+=t2.g;
t.b+=_Value1*4;
t.rg+=(1-r)*2;
float4 v= float4(t.rgb,t.a*(1-_Alpha));

o.Albedo = v.rgb * v.a;
o.Alpha = v.a;
clip(o.Alpha-0.05);
}
ENDCG
}

Fallback "Sprites/Default"
}