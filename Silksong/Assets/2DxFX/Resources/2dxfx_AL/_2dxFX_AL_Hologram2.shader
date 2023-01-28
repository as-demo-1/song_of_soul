//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Hologram2"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("_Color", Color) = (1,1,1,1)
		_Size("Size", Range(0,1)) = 0
		_Distortion("Distortion", Range(0,1)) = 0
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
	float _Size;
	float _Distortion;
	float _Alpha;
	float4 _Color;
	float _TimeX;

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

	inline float mod(float x,float modu) {
		return x - floor(x * (1.0 / modu)) * modu;
	}

	inline float noise(float2 p)
	{
			_TimeX=_Time.y;
		float samplex = tex2D(_MainTex,float2(.2,0.2*cos(_TimeX))*_TimeX*8. + p*1.).x;
		samplex *= samplex;
		return samplex;
	}


	inline float onOff(float a, float b, float c)
	{
		_TimeX=_Time.y;
		return step(c, sin(_TimeX + a*cos(_TimeX*b)));
	}

	inline float ramp(float y, float start, float end)
	{
		float inside = step(start,y) - step(end,y);
		float fact = (y - start) / (end - start)*inside;
		return (1. - fact) * inside;

	}

	inline float stripes(float2 uv)
	{
	_TimeX=_Time.y;
		float noi = noise(uv*float2(0.5,1.) + float2(6.,3.))*_Distortion * 3;
		return ramp(mod(uv.y*4. + _TimeX / 2. + sin(_TimeX + sin(_TimeX*0.63)),1.),.5,.6)*noi;
	}

	inline float4 getVideo(float2 uv)
	{
		_TimeX=_Time.y;
		float2 look = uv;
		float window = 1. / (1. + 20.*(look.y - mod(_TimeX / 4.,1.))*(look.y - mod(_TimeX / 4.,1.)));
		look.x = look.x + sin(look.y*30. + _TimeX) / (50.*_Distortion)*onOff(4.,4.,.3)*(1. + cos(_TimeX*80.))*window;

		float vShift = .4*onOff(2.,3.,.9)*(sin(_TimeX)*sin(_TimeX*20.) +
			(0.5 + 0.1*sin(_TimeX*20.)*cos(_TimeX)));

		look.y = mod(look.y + vShift, 1.);

		float4 video;
		float4 video2 = tex2D(_MainTex,look);
		video = video2;
		float onoffx = onOff(2.,1.5,.9);
		video.r = tex2D(_MainTex,look - float2(.05,0.)*onoffx).r;
		video.b = tex2D(_MainTex,look + float2(.05,0.)*onoffx).b;

		return video;
	}
	void surf(Input IN, inout SurfaceOutput o)
	{

	_TimeX=_Time.y;
		float2 uv = IN.uv_MainTex;
		float alpha = tex2D(_MainTex,uv).a;
		float4 video = getVideo(uv)* IN.color;
		float vigAmt = 3. + .3*sin(_TimeX + 5.*cos(_TimeX*5.));
		float vignette = (1. - vigAmt*(uv.y - .5)*(uv.y - .5))*(1. - vigAmt*(uv.x - .5)*(uv.x - .5));
		video += stripes(uv);
		video += noise(uv*2.) / 2.;
		video.r *= vignette;
		video.a = video.a + (frac(sin(dot(uv.xy*_TimeX,float2(12.9898,78.233))) * 43758.5453))*.5;
		video *= (12. + mod(uv.y*30. + _TimeX,1.)) / 13.;
		video.a = (video.a*.3)*alpha*vignette * 2 * (1 - _Alpha)*IN.color.a;


		o.Albedo = video.rgb * video.a;
		o.Alpha = video.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}