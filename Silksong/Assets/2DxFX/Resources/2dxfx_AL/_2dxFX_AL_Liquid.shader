//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2016 //
/// http://unity3D.vetasoft.com/            //
//////////////////////////////////////////////

Shader "2DxFX/AL/Liquid"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("_Color", Color) = (1,1,1,1)
		_Distortion("Distortion", Range(0,1)) = 0
		_Alpha("Alpha", Range(0,1)) = 1.0
		_Speed("Speed", Range(0,1)) = 1.0
		EValue("EValue", Range(0,1)) = 1.0
		Light("Light", Range(0,1)) = 1.0
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
	float _Distortion;
	float _Alpha;
	float _Speed;
	float EValue;
	float4 _Color;
	float Light;

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

	float col(float2 coord)
	{
		float time = _Time * 10;
		float delta_theta = 0.897597901025655210989326680937;
		float col = 0.0;
		float theta = 0.0;


		float _Value = _Speed;
		float _Value2 = _Distortion;
		float _Value3 = _Distortion;
		float _Value4 = _Distortion;

		float2 adjc = coord;
		theta = delta_theta * 1;
		adjc.x += cos(theta)*time*_Value + time * _Value2;
		adjc.y -= sin(theta)*time*_Value - time * _Value3;
		col = col + cos((adjc.x*cos(theta) - adjc.y*sin(theta))*6.0);
		theta = delta_theta * 2;
		adjc.x += cos(theta)*time*_Value + time * _Value2;
		adjc.y -= sin(theta)*time*_Value - time * _Value3;
		col = col + cos((adjc.x*cos(theta) - adjc.y*sin(theta))*6.0);
		theta = delta_theta * 3;
		adjc.x += cos(theta)*time*_Value + time * _Value2;
		adjc.y -= sin(theta)*time*_Value - time * _Value3;
		col = col + cos((adjc.x*cos(theta) - adjc.y*sin(theta))*6.0);
		theta = delta_theta * 4;
		adjc.x += cos(theta)*time*_Value + time * _Value2;
		adjc.y -= sin(theta)*time*_Value - time * _Value3;
		col = col + cos((adjc.x*cos(theta) - adjc.y*sin(theta))*6.0);
		theta = delta_theta * 5;
		adjc.x += cos(theta)*time*_Value + time * _Value2;
		adjc.y -= sin(theta)*time*_Value - time * _Value3;
		col = col + cos((adjc.x*cos(theta) - adjc.y*sin(theta))*6.0);
		theta = delta_theta * 6;
		adjc.x += cos(theta)*time*_Value + time * _Value2;
		adjc.y -= sin(theta)*time*_Value - time * _Value3;
		col = col + cos((adjc.x*cos(theta) - adjc.y*sin(theta))*6.0);
		theta = delta_theta * 7;
		adjc.x += cos(theta)*time*_Value + time * _Value2;
		adjc.y -= sin(theta)*time*_Value - time * _Value3;
		col = col + cos((adjc.x*cos(theta) - adjc.y*sin(theta))*6.0);
		theta = delta_theta * 8;
		adjc.x += cos(theta)*time*_Value + time * _Value2;
		adjc.y -= sin(theta)*time*_Value - time * _Value3;
		col = col + cos((adjc.x*cos(theta) - adjc.y*sin(theta))*6.0);
		return cos(col);
	}
	void surf(Input IN, inout SurfaceOutput o)
	{

		float2 p = IN.uv_MainTex, c1 = p, c2 = p;
		float cc1 = col(c1);
		c2.x += 8.53;
		float dx = 0.50*(cc1 - col(c2)) / 60;
		c2.x = p.x;
		c2.y += 8.53;
		float dy = 0.50*(cc1 - col(c2)) / 60;
		c1.x += dx*2.;
		c1.y = (c1.y + dy*2.);
		float alpha = 1. + dot(dx,dy) * 700 * Light;
		float ddx = dx - 0.012;
		float ddy = dy - 0.012;
		if (ddx > 0. && ddy > 0.) alpha = pow(alpha, ddx*ddy * 200000);
		c1 = lerp(p,c1,EValue);
		float4 col = tex2D(_MainTex,c1)*(alpha)* IN.color;
		float4 c = float4(col.rgb,col.a*(1 - _Alpha));

		o.Albedo = c.rgb * c.a;
		o.Alpha = c.a;

		clip(o.Alpha - 0.05);
	}
	ENDCG
	}

		Fallback "Transparent/VertexLit"
}