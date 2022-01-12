// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ase_disslove"
{
	Properties
	{
		_basetex("base tex", 2D) = "white" {}
		_desslovetex("desslove tex", 2D) = "white" {}
		_desaturation("desaturation", Float) = 1
		_depthfade("depth fade", Float) = 0
		_power("power", Float) = 0
		[HideInInspector] _tex4coord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Custom"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma surface surf Lambert keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
			float4 uv_tex4coord;
			float4 screenPos;
		};

		uniform sampler2D _basetex;
		uniform half4 _basetex_ST;
		uniform half _power;
		uniform half _desaturation;
		uniform sampler2D _desslovetex;
		uniform half4 _desslovetex_ST;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform half _depthfade;

		void surf( Input i , inout SurfaceOutput o )
		{
			half4 color14 = IsGammaSpace() ? half4(1,1,1,0) : half4(1,1,1,0);
			float2 uv_basetex = i.uv_texcoord * _basetex_ST.xy + _basetex_ST.zw;
			half4 tex2DNode2 = tex2D( _basetex, uv_basetex );
			half4 temp_cast_0 = (_power).xxxx;
			half4 clampResult32 = clamp( pow( tex2DNode2 , temp_cast_0 ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			half3 desaturateInitialColor6 = (clampResult32).rgb;
			half desaturateDot6 = dot( desaturateInitialColor6, float3( 0.299, 0.587, 0.114 ));
			half3 desaturateVar6 = lerp( desaturateInitialColor6, desaturateDot6.xxx, _desaturation );
			o.Emission = ( (i.vertexColor).rgb * ( (color14).rgb * desaturateVar6 ).x );
			half clampResult34 = clamp( pow( tex2DNode2.r , _power ) , 0.0 , 1.0 );
			float2 uv_desslovetex = i.uv_texcoord * _desslovetex_ST.xy + _desslovetex_ST.zw;
			half ifLocalVar4 = 0;
			if( tex2D( _desslovetex, uv_desslovetex ).r >= i.uv_tex4coord.z )
				ifLocalVar4 = 1.0;
			else
				ifLocalVar4 = 0.0;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			half4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth24 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			half distanceDepth24 = abs( ( screenDepth24 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _depthfade ) );
			o.Alpha = ( ( i.vertexColor.a * ( clampResult34 * ifLocalVar4 ) ) * distanceDepth24 );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18912
0;0;2560;1379;2378.939;505.029;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;31;-1759.343,268.5338;Inherit;False;Property;_power;power;5;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-1911.616,66.68633;Inherit;True;Property;_basetex;base tex;1;0;Create;True;0;0;0;False;0;False;-1;None;b73b4ee57c0481848836be673e3f3f0f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;29;-1557.343,3.533813;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;32;-1391.343,4.533813;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-990.1583,782.8215;Inherit;False;Constant;_Float1;Float 1;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;33;-1549.343,189.5338;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;3;-1245.616,76.68633;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-1228.441,-27.6964;Inherit;False;Property;_desaturation;desaturation;3;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-996.1583,698.8215;Inherit;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;5;-1238.916,339.9863;Inherit;True;Property;_desslovetex;desslove tex;2;0;Create;True;0;0;0;False;0;False;-1;None;db9cbc8cc906c4f4884547702b2e1f8e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;28;-1212.343,539.5338;Inherit;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;14;-967.7471,-175.4735;Inherit;False;Constant;_Color0;Color 0;4;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;34;-1344.343,186.5338;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;17;-765.7471,-85.47351;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DesaturateOpNode;6;-1040.848,80.3277;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ConditionalIfNode;4;-776.916,543.9863;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-462.1583,186.8215;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-478.7471,-22.47351;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-361.1583,414.8215;Inherit;False;Property;_depthfade;depth fade;4;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;1;-303.571,162.6506;Inherit;False;Particle_color;-1;;1;2917504f51a0d66439adf74bd9579795;0;2;1;FLOAT;0;False;5;FLOAT;0;False;2;FLOAT3;0;FLOAT;6
Node;AmplifyShaderEditor.DepthFade;24;-150.1583,350.8215;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;132.8417,179.8215;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;316,-5;Half;False;True;-1;2;ASEMaterialInspector;0;0;Lambert;ase_disslove;False;False;False;False;True;True;True;True;True;True;True;True;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Custom;;Transparent;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;29;0;2;0
WireConnection;29;1;31;0
WireConnection;32;0;29;0
WireConnection;33;0;2;1
WireConnection;33;1;31;0
WireConnection;3;0;32;0
WireConnection;34;0;33;0
WireConnection;17;0;14;0
WireConnection;6;0;3;0
WireConnection;6;1;8;0
WireConnection;4;0;5;1
WireConnection;4;1;28;3
WireConnection;4;2;21;0
WireConnection;4;3;21;0
WireConnection;4;4;22;0
WireConnection;23;0;34;0
WireConnection;23;1;4;0
WireConnection;9;0;17;0
WireConnection;9;1;6;0
WireConnection;1;1;9;0
WireConnection;1;5;23;0
WireConnection;24;0;27;0
WireConnection;25;0;1;6
WireConnection;25;1;24;0
WireConnection;0;2;1;0
WireConnection;0;9;25;0
ASEEND*/
//CHKSM=6E2B8C170BB7429442D06C60DA1B33880DAB4168