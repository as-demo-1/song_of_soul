// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "QFX/ProjectilesFX/Aura"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		[Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend", Int) = 5
		[Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend", Int) = 10
		[KeywordEnum(None,Add,Lerp)] _Blend("Blend", Float) = 0
		_EmissiveMultiply("Emissive Multiply", Float) = 1
		_OpacityMultiply("Opacity Multiply", Float) = 1
		_MainTexturePower("Main Texture Power", Float) = 1
		_Tiling("Tiling", Vector) = (1,1,1,1)
		_TimeScale1("Time Scale 1", Float) = 1
		_TimeScale2("Time Scale 2", Float) = 1
		[Toggle(_USEUVGRADIENT_ON)] _UseUVGradient("Use UV Gradient", Float) = 0

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend [_SrcBlend] [_DstBlend]
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#define ASE_NEEDS_FRAG_COLOR
				#pragma multi_compile_local _BLEND_NONE _BLEND_ADD _BLEND_LERP
				#pragma shader_feature_local _USEUVGRADIENT_ON


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform int _DstBlend;
				uniform int _SrcBlend;
				uniform float _EmissiveMultiply;
				uniform float _TimeScale1;
				uniform float4 _Tiling;
				uniform float _MainTexturePower;
				uniform float _TimeScale2;
				uniform float _OpacityMultiply;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float mulTime155 = _Time.y * _TimeScale1;
					float2 texCoord16 = i.texcoord.xy * (_Tiling).xy + float2( 0,0 );
					float2 panner12 = ( mulTime155 * float2( 1,0 ) + texCoord16);
					float temp_output_151_0 = pow( tex2D( _MainTex, panner12 ).r , _MainTexturePower );
					float mulTime156 = _Time.y * _TimeScale2;
					float2 texCoord81 = i.texcoord.xy * (_Tiling).zw + float2( 0,0 );
					float2 panner77 = ( mulTime156 * float2( 1,0 ) + texCoord81);
					float temp_output_152_0 = pow( tex2D( _MainTex, panner77 ).r , _MainTexturePower );
					float lerpResult174 = lerp( temp_output_151_0 , temp_output_152_0 , 0.5);
					#if defined(_BLEND_NONE)
					float staticSwitch136 = temp_output_151_0;
					#elif defined(_BLEND_ADD)
					float staticSwitch136 = ( temp_output_151_0 + temp_output_152_0 );
					#elif defined(_BLEND_LERP)
					float staticSwitch136 = lerpResult174;
					#else
					float staticSwitch136 = temp_output_151_0;
					#endif
					float2 texCoord17 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					#ifdef _USEUVGRADIENT_ON
					float staticSwitch139 = saturate( texCoord17.x );
					#else
					float staticSwitch139 = 1.0;
					#endif
					float4 temp_output_4_0 = ( _TintColor * saturate( staticSwitch136 ) * i.color * staticSwitch139 );
					float4 appendResult32 = (float4(( _EmissiveMultiply * float4( (temp_output_4_0).rgb , 0.0 ) * unity_ColorSpaceDouble ).rgb , saturate( ( (temp_output_4_0).a * _OpacityMultiply ) )));
					

					fixed4 col = appendResult32;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18500
316.8;534.4;952;555;4.237;112.9748;1.982551;True;False
Node;AmplifyShaderEditor.Vector4Node;59;-2871.087,300.7404;Float;False;Property;_Tiling;Tiling;6;0;Create;True;0;0;False;0;False;1,1,1,1;0.6,2.5,1,4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;153;-2505.401,531.0943;Inherit;False;Property;_TimeScale1;Time Scale 1;7;0;Create;True;0;0;False;0;False;1;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;154;-2507.218,622.1009;Inherit;False;Property;_TimeScale2;Time Scale 2;8;0;Create;True;0;0;False;0;False;1;2.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;78;-2610.451,395.8404;Inherit;False;False;False;True;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;60;-2609.597,304.1638;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;155;-2333.517,536.3231;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;16;-2390.341,263.6985;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;81;-2389.637,401.2005;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;156;-2340.333,627.3288;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;12;-2103.981,353.0476;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;1,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;2;-2094.162,236.3354;Inherit;False;0;0;_MainTex;Shader;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;77;-2095.194,495.2561;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;1,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;150;-1780.479,714.7845;Inherit;False;Property;_MainTexturePower;Main Texture Power;5;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;10;-1858.018,296.8806;Inherit;True;Property;_NoiseTexture;Noise Texture;1;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;76;-1853.515,494.4691;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;152;-1501.917,502.0537;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;151;-1502.92,375.6288;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;85;-1312.349,441.9969;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;17;-204.0986,1262.748;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;174;-1318.089,554.012;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;140;396.31,1066.574;Inherit;False;Constant;_Float0;Float 0;13;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;136;-1088.318,291.9145;Inherit;False;Property;_Blend;Blend;2;0;Create;True;0;0;False;0;False;1;0;1;True;;KeywordEnum;3;None;Add;Lerp;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;177;310.2401,1239.818;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;1;162.6038,299.0114;Inherit;False;0;0;_TintColor;Shader;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;64;-324.5661,293.9368;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;5;44.95766,548.6125;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;139;672.4987,1045.724;Inherit;False;Property;_UseUVGradient;Use UV Gradient;11;0;Create;True;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;541.1465,514.8575;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;31;805.7357,544.7939;Inherit;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;158;806.8719,638.1957;Inherit;False;Property;_OpacityMultiply;Opacity Multiply;4;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;30;816.4141,440.2418;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;116;663.2582,97.55984;Inherit;False;Property;_EmissiveMultiply;Emissive Multiply;3;0;Create;True;0;0;False;0;False;1;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorSpaceDouble;178;639.7061,198.0749;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;159;1025.4,624.8604;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;34;1163.043,625.5246;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;157;1068.995,307.3964;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;175;-2896.19,-92.175;Inherit;False;222.385;257.1656;Custom Blending;2;145;147;;1,0,0,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-987.9282,78.17453;Float;False;Property;_Cutout;Cutout;12;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;32;1480.877,497.0336;Inherit;True;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.IntNode;147;-2853.735,49.78228;Inherit;False;Property;_DstBlend;DstBlend;1;1;[Enum];Create;True;0;1;UnityEngine.Rendering.BlendMode;True;0;False;10;10;0;1;INT;0
Node;AmplifyShaderEditor.LerpOp;176;54.78989,1241.796;Inherit;True;3;0;FLOAT;-0.05;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;138;-571.1035,-10.14218;Inherit;False;Property;_UseCutout;Use Cutout;10;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;44;-796.3727,58.14494;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;167;-1278.365,1291.307;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;172;-1676.339,1286.685;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;171;-1129.369,1522.964;Inherit;False;Property;_FresnelPower;Fresnel Power;13;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;168;-1125.981,1291.885;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;99;-634.6595,1228.034;Inherit;False;Constant;_Float2;Float 2;9;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;173;-1495.125,1434.95;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;170;-804.3082,1342.518;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;98;-491.9409,1226.445;Inherit;False;Property;_UseFresnel;Use Fresnel;9;0;Create;True;0;0;False;0;False;1;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;162;-1659.864,1437.009;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.IntNode;145;-2853.216,-39.65984;Inherit;False;Property;_SrcBlend;SrcBlend;0;1;[Enum];Create;True;0;1;UnityEngine.Rendering.BlendMode;True;0;False;5;5;0;1;INT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;146;1721.52,497.3652;Float;False;True;-1;2;ASEMaterialInspector;0;7;QFX/ProjectilesFX/Aura;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;1;5;True;145;0;True;147;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;True;2;False;-1;True;True;True;True;False;0;False;-1;False;False;False;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;78;0;59;0
WireConnection;60;0;59;0
WireConnection;155;0;153;0
WireConnection;16;0;60;0
WireConnection;81;0;78;0
WireConnection;156;0;154;0
WireConnection;12;0;16;0
WireConnection;12;1;155;0
WireConnection;77;0;81;0
WireConnection;77;1;156;0
WireConnection;10;0;2;0
WireConnection;10;1;12;0
WireConnection;76;0;2;0
WireConnection;76;1;77;0
WireConnection;152;0;76;1
WireConnection;152;1;150;0
WireConnection;151;0;10;1
WireConnection;151;1;150;0
WireConnection;85;0;151;0
WireConnection;85;1;152;0
WireConnection;174;0;151;0
WireConnection;174;1;152;0
WireConnection;136;1;151;0
WireConnection;136;0;85;0
WireConnection;136;2;174;0
WireConnection;177;0;17;1
WireConnection;64;0;136;0
WireConnection;139;1;140;0
WireConnection;139;0;177;0
WireConnection;4;0;1;0
WireConnection;4;1;64;0
WireConnection;4;2;5;0
WireConnection;4;3;139;0
WireConnection;31;0;4;0
WireConnection;30;0;4;0
WireConnection;159;0;31;0
WireConnection;159;1;158;0
WireConnection;34;0;159;0
WireConnection;157;0;116;0
WireConnection;157;1;30;0
WireConnection;157;2;178;0
WireConnection;32;0;157;0
WireConnection;32;3;34;0
WireConnection;176;2;17;1
WireConnection;138;1;136;0
WireConnection;138;0;44;0
WireConnection;44;0;136;0
WireConnection;44;1;57;0
WireConnection;167;0;172;0
WireConnection;167;1;173;0
WireConnection;168;0;167;0
WireConnection;173;0;162;0
WireConnection;170;0;168;0
WireConnection;170;1;171;0
WireConnection;98;0;99;0
WireConnection;98;1;170;0
WireConnection;146;0;32;0
ASEEND*/
//CHKSM=B338DCBE6C11F41C6C644C55BE90460584894B36