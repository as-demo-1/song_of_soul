// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "两层贴图刀光"
{
	Properties
	{
		_albedo("albedo", 2D) = "white" {}
		_emission("emission", 2D) = "white" {}
		[HDR]_albedoColor("albedoColor", Color) = (1,1,1,0)
		[HDR]_emissionColor("emissionColor", Color) = (1,1,1,0)
		_EmissionDissolveTex("EmissionDissolveTex", 2D) = "white" {}
		_mainDissolveTex("mainDissolveTex", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite Off
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform sampler2D _albedo;
			uniform float4 _albedo_ST;
			uniform float4 _albedoColor;
			uniform sampler2D _emission;
			uniform float4 _emission_ST;
			uniform float4 _emissionColor;
			uniform sampler2D _EmissionDissolveTex;
			uniform float4 _EmissionDissolveTex_ST;
			uniform sampler2D _mainDissolveTex;
			uniform float4 _mainDissolveTex_ST;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord1 = v.ase_texcoord;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float2 uv_albedo = i.ase_texcoord1.xy * _albedo_ST.xy + _albedo_ST.zw;
				float4 tex2DNode1 = tex2D( _albedo, uv_albedo );
				float4 uvs4_emission = i.ase_texcoord1;
				uvs4_emission.xy = i.ase_texcoord1.xy * _emission_ST.xy + _emission_ST.zw;
				float2 uv_EmissionDissolveTex = i.ase_texcoord1.xy * _EmissionDissolveTex_ST.xy + _EmissionDissolveTex_ST.zw;
				float2 uv_mainDissolveTex = i.ase_texcoord1.xy * _mainDissolveTex_ST.xy + _mainDissolveTex_ST.zw;
				float4 appendResult26 = (float4((( ( tex2DNode1.r * _albedoColor ) + ( ( tex2D( _emission, uvs4_emission.xy ) * _emissionColor ) * step( tex2D( _EmissionDissolveTex, uv_EmissionDissolveTex ).r , (-0.1 + (uvs4_emission.z - 0.0) * (1.1 - -0.1) / (1.0 - 0.0)) ) ) )).rgb , saturate( ( tex2DNode1.r * step( tex2D( _mainDissolveTex, uv_mainDissolveTex ).r , (-0.1 + (uvs4_emission.w - 0.0) * (1.1 - -0.1) / (1.0 - 0.0)) ) ) )));
				
				
				finalColor = appendResult26;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18912
290.4;73.6;880.4;411.8;2052.939;72.44647;1.886289;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-1373.698,-61.59988;Inherit;False;0;2;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-964.4067,-73.8867;Inherit;True;Property;_emission;emission;1;0;Create;True;0;0;0;False;0;False;-1;None;9592e82d3f100dd4cbd32be2b7b5abc0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;9;-965.4501,344.2451;Inherit;True;Property;_EmissionDissolveTex;EmissionDissolveTex;4;0;Create;True;0;0;0;False;0;False;-1;None;d9e6c4c5e4220a04ebaedae1af332047;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;7;-878.6415,150.4196;Inherit;False;Property;_emissionColor;emissionColor;3;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;15;-879.0827,547.5739;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-0.1;False;4;FLOAT;1.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;10;-604.3868,384.3863;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-1050.686,-521.8314;Inherit;True;Property;_albedo;albedo;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;6;-912.9759,-293.4213;Inherit;False;Property;_albedoColor;albedoColor;2;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-587.785,72.45918;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;31;-825.6624,1014.097;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-0.1;False;4;FLOAT;1.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;17;-965.4427,766.1755;Inherit;True;Property;_mainDissolveTex;mainDissolveTex;7;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-589.3119,-373.7099;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-367.2169,165.2316;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StepOpNode;30;-564.3788,812.9755;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-87.45853,-104.1309;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-71.78911,213.5421;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;28;106.0995,184.9034;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;27;79.21466,-53.96558;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-1239.889,573.2332;Inherit;False;Property;_Float0;Float 0;5;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-1248.84,1045.682;Inherit;False;Property;_Float1;Float 1;6;0;Create;True;0;0;0;False;0;False;0;0;0;1.01;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;26;307.646,29.48544;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;471.6582,55.43646;Float;False;True;-1;2;ASEMaterialInspector;100;1;两层贴图刀光;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;2;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;2;1;29;0
WireConnection;15;0;29;3
WireConnection;10;0;9;1
WireConnection;10;1;15;0
WireConnection;5;0;2;0
WireConnection;5;1;7;0
WireConnection;31;0;29;4
WireConnection;4;0;1;1
WireConnection;4;1;6;0
WireConnection;14;0;5;0
WireConnection;14;1;10;0
WireConnection;30;0;17;1
WireConnection;30;1;31;0
WireConnection;16;0;4;0
WireConnection;16;1;14;0
WireConnection;25;0;1;1
WireConnection;25;1;30;0
WireConnection;28;0;25;0
WireConnection;27;0;16;0
WireConnection;26;0;27;0
WireConnection;26;3;28;0
WireConnection;0;0;26;0
ASEEND*/
//CHKSM=9F0AC84895C74373F25A4320E50A687AD3C7F526