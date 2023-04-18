// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "顶点扰动"
{
	Properties
	{
		_offset_diffuse("offset_diffuse", 2D) = "white" {}
		[HDR]_diffuse_color("diffuse_color", Color) = (1,1,1,0)
		_offset_intensty("offset_intensty", Float) = 1
		_dissolve_texture("dissolve_texture", 2D) = "white" {}
		_tiling("tiling", Vector) = (1,2,0,0)
		_dissolve_mask("dissolve_mask", 2D) = "white" {}
		_OffsetSpeed("OffsetSpeed", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
	LOD 100

		CGINCLUDE
		#pragma target 2.0
		ENDCG
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaToMask Off
		Cull Off
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
			#include "UnityShaderVariables.cginc"
			#define ASE_NEEDS_FRAG_COLOR


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float3 ase_normal : NORMAL;
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

			uniform sampler2D _offset_diffuse;
			uniform float2 _tiling;
			uniform float2 _OffsetSpeed;
			uniform float _offset_intensty;
			uniform float4 _diffuse_color;
			uniform sampler2D _dissolve_mask;
			uniform float4 _dissolve_mask_ST;
			uniform sampler2D _dissolve_texture;
			uniform float4 _dissolve_texture_ST;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float2 appendResult6 = (float2(( _OffsetSpeed.x * _Time.y ) , ( _OffsetSpeed.y * _Time.y )));
				float4 texCoord10 = v.ase_texcoord;
				texCoord10.xy = v.ase_texcoord.xy * _tiling + appendResult6;
				
				o.ase_color = v.color;
				o.ase_texcoord1 = v.ase_texcoord;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = ( ( tex2Dlod( _offset_diffuse, float4( texCoord10.xy, 0, 0.0) ).r * _offset_intensty ) * v.ase_normal );
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
				float2 uv_dissolve_mask = i.ase_texcoord1.xy * _dissolve_mask_ST.xy + _dissolve_mask_ST.zw;
				float2 uv_dissolve_texture = i.ase_texcoord1.xy * _dissolve_texture_ST.xy + _dissolve_texture_ST.zw;
				float2 appendResult6 = (float2(( _OffsetSpeed.x * _Time.y ) , ( _OffsetSpeed.y * _Time.y )));
				float4 texCoord10 = i.ase_texcoord1;
				texCoord10.xy = i.ase_texcoord1.xy * _tiling + appendResult6;
				float clampResult21 = clamp( ( i.ase_color.a * step( ( tex2D( _dissolve_mask, uv_dissolve_mask ).r * tex2D( _dissolve_texture, uv_dissolve_texture ).r ) , texCoord10.z ) ) , 0.0 , 1.0 );
				float4 appendResult34 = (float4((( i.ase_color * _diffuse_color )).rgb , clampResult21));
				
				
				finalColor = appendResult34;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18912
300.8;73.6;831.6;424.6;1095.083;502.2252;2.513076;True;False
Node;AmplifyShaderEditor.TimeNode;2;-1140.172,369.6728;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;29;-1072.061,182.1118;Inherit;False;Property;_OffsetSpeed;OffsetSpeed;6;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-819.1725,270.6729;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-803.1725,402.6728;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;9;-202.9785,-228.2543;Inherit;True;Property;_dissolve_texture;dissolve_texture;3;0;Create;True;0;0;0;False;0;False;-1;ee4ea2151938bc04c839b44292e78813;ee4ea2151938bc04c839b44292e78813;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;8;-204.7455,-434.0689;Inherit;True;Property;_dissolve_mask;dissolve_mask;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;27;-726.9492,25.03801;Inherit;False;Property;_tiling;tiling;4;0;Create;True;0;0;0;False;0;False;1,2;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DynamicAppendNode;6;-577.5709,252.2689;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;181.4128,-288.4677;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;10;-364.5709,36.26895;Inherit;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;17;490.2272,-263.5621;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;19;435.5271,-697.3944;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;18;438.8825,-514.3944;Inherit;False;Property;_diffuse_color;diffuse_color;1;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,0;1.505882,5.898039,23.96863,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;13;-98.57068,38.26895;Inherit;True;Property;_offset_diffuse;offset_diffuse;0;0;Create;True;0;0;0;False;0;False;-1;None;82fbee61fc2bd564e8b08db5259c13ab;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;759.663,-287.7873;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;711.5271,-589.3944;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;12;103.3526,316.6828;Inherit;False;Property;_offset_intensty;offset_intensty;2;0;Create;True;0;0;0;False;0;False;1;-1.8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;32;937.1896,-494.1845;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;489.3526,153.6831;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;16;381.3526,399.6828;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;21;954.48,-288.8821;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;34;1165.841,-396.191;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;687.3526,226.683;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;31;1296.747,-247.7894;Float;False;True;-1;2;ASEMaterialInspector;100;1;顶点扰动;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;2;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;2;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;5;0;29;1
WireConnection;5;1;2;2
WireConnection;4;0;29;2
WireConnection;4;1;2;2
WireConnection;6;0;5;0
WireConnection;6;1;4;0
WireConnection;11;0;8;1
WireConnection;11;1;9;1
WireConnection;10;0;27;0
WireConnection;10;1;6;0
WireConnection;17;0;11;0
WireConnection;17;1;10;3
WireConnection;13;1;10;0
WireConnection;25;0;19;4
WireConnection;25;1;17;0
WireConnection;22;0;19;0
WireConnection;22;1;18;0
WireConnection;32;0;22;0
WireConnection;15;0;13;1
WireConnection;15;1;12;0
WireConnection;21;0;25;0
WireConnection;34;0;32;0
WireConnection;34;3;21;0
WireConnection;20;0;15;0
WireConnection;20;1;16;0
WireConnection;31;0;34;0
WireConnection;31;1;20;0
ASEEND*/
//CHKSM=3B716923339436A19FBE737345C17D195F4C9B0F