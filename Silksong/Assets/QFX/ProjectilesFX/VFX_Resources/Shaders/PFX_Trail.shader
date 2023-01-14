// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "QFX/ProjectilesFX/Trail"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		[Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend", Int) = 5
		[Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend", Int) = 10
		_EmissiveMultiply("Emissive Multiply", Float) = 1
		_OpacityMultiply("Opacity Multiply", Float) = 1
		_MainTiling("Main Tiling", Vector) = (1,1,1,1)
		_MainTexturePower("Main Texture Power", Float) = 1
		[KeywordEnum(None,Add,Lerp)] _Blend("Blend", Float) = 0
		_TimeScale1("Time Scale 1", Float) = 1
		_TimeScale2("Time Scale 2", Float) = 1
		[Toggle]_UseTextureMaskAlpha("Use Texture Mask Alpha", Float) = 1
		_TextureMaskAlpha("Texture Mask Alpha", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

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
				uniform float4 _MainTiling;
				uniform float _MainTexturePower;
				uniform float _TimeScale2;
				uniform float _UseTextureMaskAlpha;
				uniform sampler2D _TextureMaskAlpha;
				SamplerState sampler_TextureMaskAlpha;
				uniform float4 _TextureMaskAlpha_ST;
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

					float mulTime57 = _Time.y * _TimeScale1;
					float2 texCoord122 = i.texcoord.xy * (_MainTiling).xy + float2( 0,0 );
					float2 panner66 = ( mulTime57 * float2( -1,0 ) + texCoord122);
					float4 temp_cast_0 = (_MainTexturePower).xxxx;
					float4 temp_output_244_0 = pow( tex2D( _MainTex, panner66 ) , temp_cast_0 );
					float mulTime257 = _Time.y * _TimeScale2;
					float2 texCoord252 = i.texcoord.xy * (_MainTiling).zw + float2( 0,0 );
					float2 panner255 = ( mulTime257 * float2( -1,0 ) + texCoord252);
					float4 temp_cast_1 = (_MainTexturePower).xxxx;
					float4 temp_output_266_0 = pow( tex2D( _MainTex, panner255 ) , temp_cast_1 );
					float4 lerpResult304 = lerp( temp_output_244_0 , temp_output_266_0 , 0.5);
					#if defined(_BLEND_NONE)
					float4 staticSwitch263 = temp_output_244_0;
					#elif defined(_BLEND_ADD)
					float4 staticSwitch263 = ( temp_output_244_0 + temp_output_266_0 );
					#elif defined(_BLEND_LERP)
					float4 staticSwitch263 = lerpResult304;
					#else
					float4 staticSwitch263 = temp_output_244_0;
					#endif
					float2 uv_TextureMaskAlpha = i.texcoord.xy * _TextureMaskAlpha_ST.xy + _TextureMaskAlpha_ST.zw;
					float4 temp_output_86_0 = ( staticSwitch263 * i.color * _TintColor * (( _UseTextureMaskAlpha )?( tex2D( _TextureMaskAlpha, uv_TextureMaskAlpha ).r ):( 1.0 )) * unity_ColorSpaceDouble );
					float4 appendResult187 = (float4(( _EmissiveMultiply * (temp_output_86_0).rgb ) , saturate( ( (temp_output_86_0).a * _OpacityMultiply ) )));
					

					fixed4 col = appendResult187;
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
25.6;263.2;952;548;-1286.96;213.5637;2.411076;True;False
Node;AmplifyShaderEditor.Vector4Node;220;360.6422,202.2232;Inherit;False;Property;_MainTiling;Main Tiling;4;0;Create;True;0;0;False;0;False;1,1,1,1;0.85,2,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;55;714.8511,485.7318;Inherit;False;Property;_TimeScale1;Time Scale 1;7;0;Create;True;0;0;False;0;False;1;2.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;256;713.0337,576.7383;Inherit;False;Property;_TimeScale2;Time Scale 2;8;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;221;603.6423,168.3675;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;222;591.6423,327.3676;Inherit;False;False;False;True;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;122;825.9811,141.0742;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;57;886.7346,490.9606;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;257;879.9172,581.9662;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;252;836.5831,294.0659;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;66;1184.27,143.2297;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-1,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;189;1146.005,474.4204;Inherit;True;0;0;_MainTex;Shader;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;255;1186.436,292.4738;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-1,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;245;1495.506,593.4667;Inherit;False;Property;_MainTexturePower;Main Texture Power;5;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;253;1441.979,193.4351;Inherit;True;Property;_TextureSample0;Texture Sample 0;18;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;254;1443.018,382.8735;Inherit;True;Property;_TextureSample1;Texture Sample 1;19;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;305;1811.15,504.0494;Inherit;False;Constant;_Float2;Float 2;12;0;Create;True;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;244;1796.452,285.7636;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;266;1791.378,388.826;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;304;2089.573,389.099;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;261;2084.72,269.3553;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;263;2281.49,130.8054;Inherit;False;Property;_Blend;Blend;6;0;Create;True;0;0;False;0;False;1;0;0;True;;KeywordEnum;3;None;Add;Lerp;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;213;1912.107,1011.518;Inherit;True;Property;_TextureMaskAlpha;Texture Mask Alpha;10;0;Create;True;0;0;False;0;False;-1;None;990426196cc6d264784eac08d855a648;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;218;1984.07,908.2045;Inherit;False;Constant;_Float0;Float 0;12;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorSpaceDouble;323;2698.915,831.4365;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;200;2332.997,699.5887;Inherit;False;0;0;_TintColor;Shader;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;87;2329.874,503.2277;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;217;2236.309,909.3658;Inherit;False;Property;_UseTextureMaskAlpha;Use Texture Mask Alpha;9;0;Create;True;0;0;False;0;False;1;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;322;2621.961,267.5235;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;86;2840.968,511.5445;Inherit;True;5;5;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;107;3139.859,585.8079;Inherit;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;249;3133.731,717.1058;Inherit;False;Property;_OpacityMultiply;Opacity Multiply;3;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;248;3377.808,655.0076;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;127;3148.787,277.7301;Inherit;False;Property;_EmissiveMultiply;Emissive Multiply;2;0;Create;True;0;0;False;0;False;1;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;108;3143.667,475.0054;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;318;302.6082,-199.9589;Inherit;False;222.385;257.1656;Custom Blending;2;320;319;;1,0,0,1;0;0
Node;AmplifyShaderEditor.SaturateNode;117;3524.052,653.8808;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;310;3457.903,347.2076;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.IntNode;319;354.1931,-147.9588;Inherit;False;Property;_SrcBlend;SrcBlend;0;1;[Enum];Create;True;0;1;UnityEngine.Rendering.BlendMode;True;0;False;5;5;0;1;INT;0
Node;AmplifyShaderEditor.DynamicAppendNode;187;3909.182,467.3674;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.IntNode;320;352.6082,-58.1933;Inherit;False;Property;_DstBlend;DstBlend;1;1;[Enum];Create;True;0;1;UnityEngine.Rendering.BlendMode;True;0;False;10;1;0;1;INT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;317;4080.168,467.1783;Float;False;True;-1;2;ASEMaterialInspector;0;7;QFX/ProjectilesFX/Trail;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;2;5;True;319;10;True;320;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;True;2;False;-1;True;True;True;True;False;0;False;-1;False;False;False;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;221;0;220;0
WireConnection;222;0;220;0
WireConnection;122;0;221;0
WireConnection;57;0;55;0
WireConnection;257;0;256;0
WireConnection;252;0;222;0
WireConnection;66;0;122;0
WireConnection;66;1;57;0
WireConnection;255;0;252;0
WireConnection;255;1;257;0
WireConnection;253;0;189;0
WireConnection;253;1;66;0
WireConnection;254;0;189;0
WireConnection;254;1;255;0
WireConnection;244;0;253;0
WireConnection;244;1;245;0
WireConnection;266;0;254;0
WireConnection;266;1;245;0
WireConnection;304;0;244;0
WireConnection;304;1;266;0
WireConnection;304;2;305;0
WireConnection;261;0;244;0
WireConnection;261;1;266;0
WireConnection;263;1;244;0
WireConnection;263;0;261;0
WireConnection;263;2;304;0
WireConnection;217;0;218;0
WireConnection;217;1;213;1
WireConnection;322;0;263;0
WireConnection;86;0;322;0
WireConnection;86;1;87;0
WireConnection;86;2;200;0
WireConnection;86;3;217;0
WireConnection;86;4;323;0
WireConnection;107;0;86;0
WireConnection;248;0;107;0
WireConnection;248;1;249;0
WireConnection;108;0;86;0
WireConnection;117;0;248;0
WireConnection;310;0;127;0
WireConnection;310;1;108;0
WireConnection;187;0;310;0
WireConnection;187;3;117;0
WireConnection;317;0;187;0
ASEEND*/
//CHKSM=69B6C290A6FCF978340DF6C42A2D2B248B336117