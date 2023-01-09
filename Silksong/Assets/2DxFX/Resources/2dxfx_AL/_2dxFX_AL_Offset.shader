//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Offset"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
_Color ("_Color", Color) = (1,1,1,1)
_OffsetX ("Offset X", Range(0,1)) = 1
_OffsetY ("Offset Y", Range(0,1)) = 1
_ZoomX ("Zoom X", Range(0,1)) = 1
_ZoomY ("Zoom Y", Range(0,1)) = 1
_Alpha ("Alpha", Range (0,1)) = 1.0
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
float4 _Color;
float _Size;
float _OffsetX;
float _OffsetY;
float _ZoomX;
float _ZoomY;
float _Alpha;

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
float2 uv = IN.uv_MainTex+float2(_OffsetX,_OffsetY);
uv = fmod(uv * float2(_ZoomX ,_ZoomY),1);
float4 mainColor = tex2D(_MainTex, uv)*IN.color;
mainColor.a = mainColor.a-_Alpha;
o.Albedo = mainColor.rgb * mainColor.a;
o.Alpha = mainColor.a;
clip(o.Alpha-0.05);
}
ENDCG
}

Fallback "Sprites/Default"
}