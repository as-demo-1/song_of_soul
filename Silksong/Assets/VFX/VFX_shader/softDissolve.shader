// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "basic/softDissolve"
{
	Properties
	{
		_mainTex("mainTex", 2D) = "white" {}
		_noiseTex("noiseTex", 2D) = "white" {}
		[HDR]_Color0("Color 0", Color) = (1,1,1,0)
		_Float0("Float 0", Range( 0 , 1)) = 1
		[Toggle(_KEYWORD0_ON)] _Keyword0("使用自定义数据w", Float) = 0
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
			#define ASE_NEEDS_FRAG_COLOR
			#pragma shader_feature_local _KEYWORD0_ON


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

			uniform sampler2D _mainTex;
			uniform float4 _Color0;
			uniform sampler2D _noiseTex;
			uniform float4 _noiseTex_ST;
			uniform float _Float0;

			
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
				float4 texCoord14 = i.ase_texcoord1;
				texCoord14.xy = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float4 tex2DNode1 = tex2D( _mainTex, texCoord14.xy );
				float2 uv_noiseTex = i.ase_texcoord1.xy * _noiseTex_ST.xy + _noiseTex_ST.zw;
				#ifdef _KEYWORD0_ON
				float staticSwitch13 = texCoord14.w;
				#else
				float staticSwitch13 = _Float0;
				#endif
				float4 appendResult10 = (float4((( i.ase_color * tex2DNode1 * _Color0 )).rgb , saturate( ( i.ase_color.a * tex2DNode1.r * ( tex2D( _noiseTex, uv_noiseTex ).r + 1.0 + ( staticSwitch13 * -2.01 ) ) ) )));
				
				
				finalColor = appendResult10;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18912
315.2;73.6;875.6;433.4;2627.555;270.2501;2.55861;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;14;-1555.093,264.9198;Inherit;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;4;-1570.427,479.5634;Inherit;False;Property;_Float0;Float 0;3;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;13;-1241.017,491.0658;Inherit;False;Property;_Keyword0;使用自定义数据w;4;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-1153.541,193.4509;Inherit;True;Property;_noiseTex;noiseTex;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-904.0617,495.4588;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-2.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;11;-526.77,-146.239;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-877.7679,-39.79593;Inherit;True;Property;_mainTex;mainTex;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;6;-601.9764,127.536;Inherit;False;Property;_Color0;Color 0;2;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;3;-712.5648,347.7477;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-256.9559,0.7302856;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-256.161,151.4227;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;8;-108.3806,1.688652;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;12;-66.12225,149.3569;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;10;108.3792,39.04924;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;284.7561,68.65527;Float;False;True;-1;2;ASEMaterialInspector;100;1;basic/softDissolve;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;2;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;13;1;4;0
WireConnection;13;0;14;4
WireConnection;5;0;13;0
WireConnection;1;1;14;0
WireConnection;3;0;2;1
WireConnection;3;2;5;0
WireConnection;7;0;11;0
WireConnection;7;1;1;0
WireConnection;7;2;6;0
WireConnection;9;0;11;4
WireConnection;9;1;1;1
WireConnection;9;2;3;0
WireConnection;8;0;7;0
WireConnection;12;0;9;0
WireConnection;10;0;8;0
WireConnection;10;3;12;0
WireConnection;0;0;10;0
ASEEND*/
//CHKSM=A07AD037B2FEC72E6AE1B16F89899B014B8EE53E