// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "cano/Soft_Dissolve"
{
	Properties
	{
		_mainTex("mainTex", 2D) = "white" {}
		_noiseTex("noiseTex", 2D) = "white" {}
		[HDR]_Color0("Color 0", Color) = (1,1,1,1)
		[Enum(UnityEngine.Rendering.CullMode)]_cull_mode("cull_mode", Float) = 2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"

			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#define ASE_NEEDS_FRAG_COLOR


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
				float4 ase_color : COLOR;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float _cull_mode;
			uniform sampler2D _mainTex;
			uniform float4 _mainTex_ST;
			uniform float4 _Color0;
			uniform sampler2D _noiseTex;
			uniform float4 _noiseTex_ST;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_color = v.color;
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
				float4 uvs4_mainTex = i.ase_texcoord1;
				uvs4_mainTex.xy = i.ase_texcoord1.xy * _mainTex_ST.xy + _mainTex_ST.zw;
				float4 tex2DNode5 = tex2D( _mainTex, uvs4_mainTex.xy );
				float2 uv_noiseTex = i.ase_texcoord1.xy * _noiseTex_ST.xy + _noiseTex_ST.zw;
				float4 appendResult22 = (float4((( i.ase_color * float4( (tex2DNode5).rgb , 0.0 ) * _Color0 )).rgb , ( tex2DNode5.a * ( tex2D( _noiseTex, uv_noiseTex ).r + 1.0 + ( (-0.1 + (uvs4_mainTex.z - 0.0) * (1.1 - -0.1) / (1.0 - 0.0)) * -2.0 ) ) * i.ase_color.a * _Color0.a )));
				
				
				finalColor = appendResult22;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.TextureCoordinatesNode;14;-1564.115,-185.2092;Inherit;False;0;5;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;13;-1310.082,-21.68536;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-0.1;False;4;FLOAT;1.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-1218.092,377.9161;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;7;-1205.547,141.9325;Inherit;True;Property;_noiseTex;noiseTex;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;19;-866.2839,110.4166;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;5;-1051.93,-183.6637;Inherit;True;Property;_mainTex;mainTex;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;15;-638.2711,-555.6345;Inherit;False;Property;_Color0;Color 0;3;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;4,4,4,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;11;-755.2203,-347.4487;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-327.2046,-258.5651;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1226.367,-637.1073;Inherit;False;Property;_cull_mode;cull_mode;4;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-1509.407,375.014;Inherit;False;Property;_Float0;Float 0;2;0;Create;True;0;0;0;False;0;False;1.1;0.4620855;-0.1;1.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;9;-567.307,-142.0925;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-358.5956,35.09579;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;20;235.7908,-117.1829;Float;False;True;-1;2;ASEMaterialInspector;100;5;cano/Soft_Dissolve;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.DynamicAppendNode;22;41.33002,-116.132;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ComponentMaskNode;21;-164.1498,-186.8491;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
WireConnection;13;0;14;3
WireConnection;18;0;13;0
WireConnection;19;0;7;1
WireConnection;19;2;18;0
WireConnection;5;1;14;0
WireConnection;12;0;11;0
WireConnection;12;1;9;0
WireConnection;12;2;15;0
WireConnection;9;0;5;0
WireConnection;10;0;5;4
WireConnection;10;1;19;0
WireConnection;10;2;11;4
WireConnection;10;3;15;4
WireConnection;20;0;22;0
WireConnection;22;0;21;0
WireConnection;22;3;10;0
WireConnection;21;0;12;0
ASEEND*/
//CHKSM=91A31B33A56B8FC7D7DA367181DD1A5B5F8E4F7E