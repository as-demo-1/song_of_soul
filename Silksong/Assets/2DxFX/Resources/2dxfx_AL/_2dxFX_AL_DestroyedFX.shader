//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/DestroyedFX"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("Tint", Color) = (1,1,1,1)
		_Distortion("Distortion", Range(0,1)) = 0
		_Size("Size", Range(0,1)) = 0
		_Alpha("Alpha", Range(0,1)) = 1.0
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
		ZWrite[_Z]
		BlendOp[_BlendOp]
		Blend[_SrcBlend][_DstBlend]

		CGPROGRAM
#pragma surface surf Lambert vertex:vert nofog keepalpha addshadow fullforwardshadows
#pragma target 3.0
		sampler2D _MainTex;
	float4 _Color;
	float _Distortion;
	float _Alpha;
	float _Size;

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
	float r(float2 c) {
		return frac(43.*sin(c.x + 7.*c.y)*_Size);
	}

	float n(float2 p) {
		float2 i = floor(p), w = p - i, j = float2 (1.,0.);
		w = w*w*(3. - w - w);
		return lerp(lerp(r(i), r(i + j), w.x), lerp(r(i + j.yx), r(i + 1.), w.x), w.y);
	}

	float a(float2 p) {
		float m = 0., f = 2.;
		for (int i = 0; i<9; i++) { m += n(f*p) / f; f += f; }
		return m;
	}
	void surf(Input IN, inout SurfaceOutput o)
	{

		float2 uv = IN.uv_MainTex;
		float4 tex = tex2D(_MainTex, uv)* IN.color;
		float t = frac(_Distortion*0.9999);
		float4 c = smoothstep(t / 1.2, t + .1, a(3.5*uv));
		c = tex*c;
		c.r = lerp(c.r,c.r*15.0*(1 - c.a) * 8,_Distortion);
		c.g = lerp(c.g,c.g*10.0*(1 - c.a) * 4,_Distortion);
		c.b = lerp(c.b,c.b*5.0*(1 - c.a),_Distortion);
		float4 r = float4(c.rgb,c.a * 1 - _Alpha);
		o.Albedo = r.rgb * r.a;
		o.Alpha = r.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}