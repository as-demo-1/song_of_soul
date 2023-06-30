// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "copyShader"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.BlendMode)]_Src("SrcBlend", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)]_Dst("DstBlend", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Float) = 0
		[Enum(On,1,Off,0)]_ZWrite("ZWrite", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Float) = 4
		[HDR]_Color1("主贴图颜色", Color) = (1,1,1,1)
		_MainTex1("主帖图", 2D) = "white" {}
		[Toggle(_DESATURATE1_ON)] _Desaturate1("去色", Float) = 0
		[Toggle(_DEBLACKBG1_ON)] _DeBlackBG1("去黑", Float) = 0
		_MainTexPannerX1("主帖图U方向速度", Float) = 0
		_MainTexPannerY1("主帖图V方向速度", Float) = 0
		_TurbulenceTex1("扭曲贴图", 2D) = "white" {}
		_TurbulenceTexPannerX1("    扭曲图U方向速度", Float) = 0
		_TurbulenceTexPannerY1("    扭曲图V方向速度", Float) = 0
		[KeywordEnum(UV,U,V)] _TurbulenceDirection3("    扭曲方向", Float) = 0
		[KeywordEnum(UV,U,V)] _TurbulenceDirection("    TurbulenceDirection", Float) = 0
		_TurbulenceStrength1("    扭曲强度(Custom1.z)", Float) = 0
		_MaskTex1("遮罩图", 2D) = "white" {}
		_MaskTexPannerX1("    遮罩图U方向速度", Float) = 0
		_MaskTexPannerY1("    遮罩图V方向速度", Float) = 0
		_DissolveTex1("溶解图", 2D) = "white" {}
		[Toggle(_KEYWORD1_ON)] _Keyword1("Keyword 1", Float) = 0
		[Toggle(_KEYWORD0_ON)] _Keyword0("Keyword 0", Float) = 0
		[KeywordEnum(Particle,Material)] _DissolveMode1("    溶解模式", Float) = 1
		[Toggle(_ONEMINUS_DISSOLVETEX1_ON)] _OneMinus_DissolveTex1("    溶解图反向", Float) = 0
		_DissolveTexPannerX1("    溶解图U方向速度", Float) = 0
		_DissolveTexPannerY1("    溶解图V方向速度", Float) = 0
		_Dissolve1("    溶解值(Custom1.w)", Range( 0 , 1)) = 0
		_Hardness1("    硬度", Range( 0 , 0.99)) = 0
		[HDR]_EdgeColor1("    描边颜色(Custom2.color)", Color) = (1,1,1,1)
		_EdgeWidth1("    描边宽度", Range( 0 , 1)) = 0
		[Toggle(_ISFRESNEL_ON)] _IsFresnel("IsFresnel", Float) = 0
		[Toggle(_ISFRESNEL2_ON)] _IsFresnel2("开启Fresnel", Float) = 0
		[KeywordEnum(Add,Multiply)] _Fresnel3("开启Fresnel", Float) = 0
		[HDR]_FresnelColor("    Fresnel颜色", Color) = (1,1,1,1)
		_FresnelScale("    Fresnel强度", Float) = 1
		[Toggle(_ISDOUBLEFACE1_ON)] _IsDoubleFace1("开启双面异色", Float) = 0
		[HDR]_FaceInColor("    内面颜色", Color) = (1,1,1,1)
		[HDR]_FaceOutColor("    外面颜色", Color) = (1,1,1,1)
		[Toggle(_ISVERTEXOFFSET1_ON)] _IsVertexOffset1("开启顶点偏移", Float) = 0
		_VerTex1("    顶点偏移图", 2D) = "white" {}
		_VerTexScale1("    偏移强度", Float) = 0
		_VerTexPannerX1("    偏移U方向速度", Float) = 0
		_VerTexPannerY1("    偏移V方向速度", Float) = 0

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "PreviewType"="Plane" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend [_Src] [_Dst]
		AlphaToMask Off
		Cull [_CullMode]
		ColorMask RGBA
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			#if defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) || defined(SHADER_API_D3D9)
			#define FRONT_FACE_SEMANTIC VFACE
			#define FRONT_FACE_TYPE float
			#else
			#define FRONT_FACE_SEMANTIC SV_IsFrontFace
			#define FRONT_FACE_TYPE bool
			#endif


			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_COLOR
			#pragma shader_feature_local _ISVERTEXOFFSET1_ON
			#pragma shader_feature_local _ISFRESNEL2_ON
			#pragma shader_feature_local _ISDOUBLEFACE1_ON
			#pragma shader_feature_local _DESATURATE1_ON
			#pragma shader_feature_local _TURBULENCEDIRECTION3_UV _TURBULENCEDIRECTION3_U _TURBULENCEDIRECTION3_V
			#pragma shader_feature_local _ONEMINUS_DISSOLVETEX1_ON
			#pragma shader_feature_local _DISSOLVEMODE1_PARTICLE _DISSOLVEMODE1_MATERIAL
			#pragma shader_feature_local _FRESNEL3_ADD _FRESNEL3_MULTIPLY
			#pragma shader_feature_local _DEBLACKBG1_ON


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_color : COLOR;
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float _Dst;
			uniform float _CullMode;
			uniform float _Src;
			uniform float _ZWrite;
			uniform float _ZTest;
			uniform half _VerTexScale1;
			uniform sampler2D _VerTex1;
			uniform half _VerTexPannerX1;
			uniform half _VerTexPannerY1;
			uniform half4 _FresnelColor;
			uniform float _FresnelScale;
			uniform float4 _FaceInColor;
			uniform float4 _FaceOutColor;
			uniform float4 _Color1;
			uniform sampler2D _MainTex1;
			uniform float _MainTexPannerX1;
			uniform float _MainTexPannerY1;
			uniform float _TurbulenceStrength1;
			uniform sampler2D _TurbulenceTex1;
			uniform float _TurbulenceTexPannerX1;
			uniform float _TurbulenceTexPannerY1;
			uniform half4 _EdgeColor1;
			uniform half _Hardness1;
			uniform sampler2D _DissolveTex1;
			uniform float _DissolveTexPannerX1;
			uniform float _DissolveTexPannerY1;
			uniform half _Dissolve1;
			uniform half _EdgeWidth1;
			uniform sampler2D _MaskTex1;
			uniform float _MaskTexPannerX1;
			uniform float _MaskTexPannerY1;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float3 temp_cast_0 = (0.0).xxx;
				float2 appendResult266 = (float2(_VerTexPannerX1 , _VerTexPannerY1));
				float2 texCoord262 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner268 = ( 1.0 * _Time.y * appendResult266 + texCoord262);
				#ifdef _ISVERTEXOFFSET1_ON
				float3 staticSwitch285 = ( _VerTexScale1 * tex2Dlod( _VerTex1, float4( panner268, 0, 0.0) ).r * v.ase_normal );
				#else
				float3 staticSwitch285 = temp_cast_0;
				#endif
				
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord1.xyz = ase_worldNormal;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				o.ase_texcoord3 = v.ase_texcoord1;
				o.ase_color = v.color;
				o.ase_texcoord4 = v.ase_texcoord2;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.w = 0;
				o.ase_texcoord2.zw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = staticSwitch285;
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
			
			fixed4 frag (v2f i , FRONT_FACE_TYPE ase_vface : FRONT_FACE_SEMANTIC) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float4 temp_cast_0 = (0.0).xxxx;
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(WorldPosition);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 ase_worldNormal = i.ase_texcoord1.xyz;
				float fresnelNdotV227 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode227 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV227, _FresnelScale ) );
				#ifdef _ISFRESNEL2_ON
				float4 staticSwitch277 = ( _FresnelColor * saturate( fresnelNode227 ) );
				#else
				float4 staticSwitch277 = temp_cast_0;
				#endif
				float4 temp_cast_1 = (1.0).xxxx;
				float4 lerpResult237 = lerp( _FaceInColor , _FaceOutColor , (ase_vface*0.5 + 0.5));
				#ifdef _ISDOUBLEFACE1_ON
				float4 staticSwitch267 = lerpResult237;
				#else
				float4 staticSwitch267 = temp_cast_1;
				#endif
				float2 appendResult206 = (float2(_MainTexPannerX1 , _MainTexPannerY1));
				float2 appendResult154 = (float2(_TurbulenceTexPannerX1 , _TurbulenceTexPannerY1));
				float2 texCoord153 = i.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner155 = ( 1.0 * _Time.y * appendResult154 + texCoord153);
				float4 tex2DNode156 = tex2D( _TurbulenceTex1, panner155 );
				float4 texCoord294 = i.ase_texcoord3;
				texCoord294.xy = i.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_161_0 = ( _TurbulenceStrength1 * ( ( tex2DNode156.r * tex2DNode156.a ) - 0.5 ) * ( 1.0 - texCoord294.z ) );
				float2 texCoord293 = i.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult174 = (float2(( texCoord293.x + temp_output_161_0 ) , texCoord293.y));
				float2 appendResult168 = (float2(texCoord293.x , ( temp_output_161_0 + texCoord293.y )));
				#if defined(_TURBULENCEDIRECTION3_UV)
				float2 staticSwitch290 = ( temp_output_161_0 + texCoord293 );
				#elif defined(_TURBULENCEDIRECTION3_U)
				float2 staticSwitch290 = appendResult174;
				#elif defined(_TURBULENCEDIRECTION3_V)
				float2 staticSwitch290 = appendResult168;
				#else
				float2 staticSwitch290 = ( temp_output_161_0 + texCoord293 );
				#endif
				float2 appendResult186 = (float2(texCoord294.x , texCoord294.y));
				#ifdef _KEYWORD1_ON
				float2 staticSwitch196 = float2( 0,0 );
				#else
				float2 staticSwitch196 = appendResult186;
				#endif
				float2 panner292 = ( 1.0 * _Time.y * appendResult206 + ( staticSwitch290 + staticSwitch196 ));
				float4 tex2DNode291 = tex2D( _MainTex1, panner292 );
				float3 desaturateInitialColor224 = tex2DNode291.rgb;
				float desaturateDot224 = dot( desaturateInitialColor224, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar224 = lerp( desaturateInitialColor224, desaturateDot224.xxx, 1.0 );
				#ifdef _DESATURATE1_ON
				float4 staticSwitch233 = float4( desaturateVar224 , 0.0 );
				#else
				float4 staticSwitch233 = tex2DNode291;
				#endif
				float4 texCoord243 = i.ase_texcoord4;
				texCoord243.xy = i.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _KEYWORD0_ON
				float4 staticSwitch260 = _EdgeColor1;
				#else
				float4 staticSwitch260 = texCoord243;
				#endif
				float2 appendResult176 = (float2(_DissolveTexPannerX1 , _DissolveTexPannerY1));
				float2 texCoord162 = i.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				half Turbulence289 = temp_output_161_0;
				float2 appendResult171 = (float2(( texCoord162.x + Turbulence289 ) , texCoord162.y));
				float2 appendResult172 = (float2(texCoord162.x , ( texCoord162.y + Turbulence289 )));
				#if defined(_TURBULENCEDIRECTION_UV)
				float2 staticSwitch178 = ( texCoord162 + Turbulence289 );
				#elif defined(_TURBULENCEDIRECTION_U)
				float2 staticSwitch178 = appendResult171;
				#elif defined(_TURBULENCEDIRECTION_V)
				float2 staticSwitch178 = appendResult172;
				#else
				float2 staticSwitch178 = ( texCoord162 + Turbulence289 );
				#endif
				float2 panner187 = ( 1.0 * _Time.y * appendResult176 + staticSwitch178);
				float4 tex2DNode189 = tex2D( _DissolveTex1, panner187 );
				#ifdef _ONEMINUS_DISSOLVETEX1_ON
				float staticSwitch216 = ( 1.0 - tex2DNode189.r );
				#else
				float staticSwitch216 = tex2DNode189.r;
				#endif
				float4 texCoord179 = i.ase_texcoord3;
				texCoord179.xy = i.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				#if defined(_DISSOLVEMODE1_PARTICLE)
				float staticSwitch192 = texCoord179.w;
				#elif defined(_DISSOLVEMODE1_MATERIAL)
				float staticSwitch192 = _Dissolve1;
				#else
				float staticSwitch192 = _Dissolve1;
				#endif
				half Hardness185 = _Hardness1;
				float smoothstepResult238 = smoothstep( _Hardness1 , 1.0 , ( ( staticSwitch216 + 1.0 ) - ( ( ( staticSwitch192 * ( 1.0 + _EdgeWidth1 ) ) - _EdgeWidth1 ) * ( 1.0 + ( 1.0 - Hardness185 ) ) ) ));
				float smoothstepResult235 = smoothstep( _Hardness1 , 1.0 , ( ( staticSwitch216 + 1.0 ) - ( ( 1.0 + ( 1.0 - Hardness185 ) ) * staticSwitch192 ) ));
				float4 lerpResult269 = lerp( ( _Color1 * staticSwitch233 * i.ase_color ) , staticSwitch260 , ( smoothstepResult238 - smoothstepResult235 ));
				#ifdef _ISFRESNEL_ON
				float staticSwitch258 = ( _FresnelColor.a * saturate( fresnelNode227 ) * i.ase_color.a );
				#else
				float staticSwitch258 = 0.0;
				#endif
				#ifdef _DEBLACKBG1_ON
				float staticSwitch232 = tex2DNode291.r;
				#else
				float staticSwitch232 = tex2DNode291.a;
				#endif
				float2 appendResult211 = (float2(_MaskTexPannerX1 , _MaskTexPannerY1));
				float2 texCoord180 = i.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult203 = (float2(( texCoord180.x + Turbulence289 ) , texCoord180.y));
				float2 appendResult202 = (float2(texCoord180.x , ( texCoord180.y + Turbulence289 )));
				#if defined(_TURBULENCEDIRECTION_UV)
				float2 staticSwitch212 = ( Turbulence289 + texCoord180 );
				#elif defined(_TURBULENCEDIRECTION_U)
				float2 staticSwitch212 = appendResult203;
				#elif defined(_TURBULENCEDIRECTION_V)
				float2 staticSwitch212 = appendResult202;
				#else
				float2 staticSwitch212 = ( Turbulence289 + texCoord180 );
				#endif
				float2 panner223 = ( 1.0 * _Time.y * appendResult211 + staticSwitch212);
				float4 tex2DNode230 = tex2D( _MaskTex1, panner223 );
				float temp_output_259_0 = ( _Color1.a * staticSwitch232 * i.ase_color.a * ( tex2DNode230.r * tex2DNode230.a ) * smoothstepResult238 );
				float temp_output_271_0 = ( staticSwitch258 + temp_output_259_0 );
				#if defined(_FRESNEL3_ADD)
				float staticSwitch279 = temp_output_271_0;
				#elif defined(_FRESNEL3_MULTIPLY)
				float staticSwitch279 = ( temp_output_271_0 * temp_output_259_0 );
				#else
				float staticSwitch279 = temp_output_271_0;
				#endif
				float4 appendResult287 = (float4(( staticSwitch277 + ( staticSwitch267 * lerpResult269 ) ).rgb , staticSwitch279));
				
				
				finalColor = appendResult287;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18912
267.2;73.6;923.6;455;3357.558;927.3257;2.906755;True;False
Node;AmplifyShaderEditor.CommentaryNode;148;-4980.73,-520.3276;Inherit;False;2079.259;652.5637;扭曲+UV滚动;29;294;293;292;290;206;201;198;196;195;193;186;184;182;174;169;168;167;164;161;160;159;158;157;156;155;154;153;152;151;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;151;-4971.103,-230.1853;Inherit;False;Property;_TurbulenceTexPannerY1;    扭曲图V方向速度;13;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;152;-4972.103,-329.1853;Inherit;False;Property;_TurbulenceTexPannerX1;    扭曲图U方向速度;12;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;153;-4808.682,-475.2552;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;154;-4730.624,-307.5999;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;155;-4562.144,-409.1933;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;156;-4368.643,-437.5551;Inherit;True;Property;_TurbulenceTex1;扭曲贴图;11;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;157;-4070.346,-383.3076;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;294;-4233.854,-144.8586;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;158;-3984.073,-490.7686;Inherit;False;Property;_TurbulenceStrength1;    扭曲强度(Custom1.z);17;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;159;-3985.268,-270.7785;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;160;-3901.02,-382.4256;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;161;-3746.322,-406.1897;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;289;-3576.143,-603.2465;Half;False;Turbulence;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;293;-3837.671,-253.6033;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;162;-4834.511,538.2728;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;163;-4800.073,459.1912;Inherit;False;289;Turbulence;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;164;-3572.179,-429.1284;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;165;-4550.702,420.4285;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;167;-3573.502,-315.9727;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;166;-4553.269,524.042;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;171;-4365.757,420.9682;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;174;-3409.292,-433.2059;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;149;-3928.929,631.11;Inherit;False;1906.367;735.2061;溶解;27;252;238;235;228;225;221;220;218;217;216;215;214;213;208;207;205;204;199;194;192;190;189;185;183;181;179;177;;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;172;-4367.225,516.2001;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;175;-4337.822,646.8027;Inherit;False;Property;_DissolveTexPannerX1;    溶解图U方向速度;26;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;170;-4551.107,615.4727;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;169;-3406.027,-243.7743;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;168;-3406.301,-338.5999;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;173;-4334.298,739.7553;Inherit;False;Property;_DissolveTexPannerY1;    溶解图V方向速度;27;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;176;-4125.579,652.5688;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;177;-2798.342,902.8133;Half;False;Property;_Hardness1;    硬度;29;0;Create;False;0;0;0;False;0;False;0;0.921;0;0.99;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;178;-4188.558,462.9921;Inherit;False;Property;_TurbulenceDirection;    TurbulenceDirection;16;0;Create;True;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;UV;U;V;Reference;-1;True;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;290;-3260.624,-395.1943;Inherit;False;Property;_TurbulenceDirection3;    扭曲方向;15;0;Create;False;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;UV;U;V;Create;True;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;184;-3609.844,22.38065;Inherit;False;Constant;_Vector0;Vector 0;40;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;181;-3919.457,999.0803;Half;False;Property;_Dissolve1;    溶解值(Custom1.w);28;0;Create;False;0;0;0;False;0;False;0;0.236;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;179;-3862.407,811.1957;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;180;-3823.419,301.1403;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;182;-3030.125,-289.6961;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;183;-3576.869,1098.989;Half;False;Property;_EdgeWidth1;    描边宽度;31;0;Create;False;0;0;0;False;0;False;0;0.077;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;185;-2463.126,902.4573;Half;False;Hardness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;187;-3876.895,466.9486;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;188;-3791.933,223.8372;Inherit;False;289;Turbulence;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;186;-3747.135,-118.4207;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;195;-3431.115,90.87368;Inherit;False;Property;_MainTexPannerY1;主帖图V方向速度;10;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;189;-3694.235,656.3164;Inherit;True;Property;_DissolveTex1;溶解图;21;0;Create;False;0;0;0;False;0;False;-1;None;d9e6c4c5e4220a04ebaedae1af332047;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;190;-3306.489,1031.816;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;191;-3547.494,281.592;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;192;-3619.654,891.5451;Inherit;False;Property;_DissolveMode1;    溶解模式;24;0;Create;False;0;0;0;False;0;False;0;1;1;True;;KeywordEnum;2;Particle;Material;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;193;-3434.639,-2.078942;Inherit;False;Property;_MainTexPannerX1;主帖图U方向速度;9;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;194;-3436.802,1293.027;Inherit;False;185;Hardness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;196;-3420.844,-126.6194;Inherit;False;Property;_Keyword1;Keyword 1;22;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;-1;True;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;197;-3544.928,177.9786;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;198;-3222.928,-279.8302;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;199;-3437.893,842.7715;Inherit;False;185;Hardness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;206;-3250.995,2.387122;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;200;-3545.401,373.8059;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;201;-3191.675,-244.2355;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;202;-3371.173,271.5896;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;203;-3369.705,178.5183;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;207;-3382.813,747.5459;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;204;-3262.033,1298.244;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;209;-3275.005,486.9686;Inherit;False;Property;_MaskTexPannerY1;    遮罩图V方向速度;20;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;210;-3278.734,403.4009;Inherit;False;Property;_MaskTexPannerX1;    遮罩图U方向速度;19;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;208;-3177.489,972.8159;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;205;-3263.123,847.9883;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;211;-3058.789,409.0058;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;212;-3187.536,219.4529;Inherit;False;Property;_TurbulenceDirection;    TurbulenceDirection;14;0;Create;True;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;UV;U;V;Reference;-1;True;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;213;-3105.718,1274.5;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;214;-3105.258,824.2453;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;215;-3069.438,1087.336;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;216;-3228.874,681.2086;Inherit;False;Property;_OneMinus_DissolveTex1;    溶解图反向;25;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;292;-3065.294,-125.642;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FaceVariableNode;222;-1766.45,-353.9538;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;219;-2076.778,-879.4517;Inherit;False;Property;_FresnelScale;    Fresnel强度;36;0;Create;False;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;220;-2964.695,825.4644;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;218;-2902.699,1252.079;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;291;-2895.538,-153.3272;Inherit;True;Property;_MainTex1;主帖图;6;0;Create;False;0;0;0;False;0;False;-1;None;eb9ccdd3d95852a4bbc4c9e3251a60c4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;221;-2915.509,1091.095;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;223;-2873.089,280.9767;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;217;-2901.519,978.5593;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;224;-2549.865,-129.6853;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;225;-2728.334,990.1687;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;228;-2724.751,1094.609;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;229;-1657.275,-353.2918;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;230;-2677.092,252.9291;Inherit;True;Property;_MaskTex1;遮罩图;18;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;226;-1737.664,-739.1584;Inherit;False;Property;_FaceInColor;    内面颜色;38;1;[HDR];Create;False;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;231;-1735.902,-523.39;Inherit;False;Property;_FaceOutColor;    外面颜色;39;1;[HDR];Create;False;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;227;-1893.475,-971.4543;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;242;-1555.916,-1403.818;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;239;-1600.724,-1212.026;Half;False;Property;_FresnelColor;    Fresnel颜色;35;1;[HDR];Create;False;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;232;-2441.706,-29.04958;Inherit;False;Property;_DeBlackBG1;去黑;8;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;235;-2491.027,668.1809;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;234;-2394.325,92.17379;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;236;-2382.586,303.8646;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;238;-2485.674,1066.346;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;237;-1440.225,-542.4573;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;240;-1528.621,-1041.585;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;241;-2378.995,-354.3272;Inherit;False;Property;_Color1;主贴图颜色;5;1;[HDR];Create;False;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;233;-2368.036,-158.3411;Inherit;False;Property;_Desaturate1;去色;7;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;244;-1405.495,426.8734;Half;False;Property;_VerTexPannerX1;    偏移U方向速度;43;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;247;-1303.089,-1105.321;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;252;-2240.57,843.9505;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;245;-2152.673,156.2069;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;255;-2152.731,0.1419439;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;248;-2154.137,64.11501;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;249;-2171.131,206.4748;Half;False;Property;_EdgeColor1;    描边颜色(Custom2.color);30;1;[HDR];Create;False;0;0;0;False;0;False;1,1,1,1;2.118547,1.608321,1.098095,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;250;-2117.71,-176.9624;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;251;-1348.376,-196.4747;Inherit;False;Constant;_Float0;Float 0;29;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;243;-2165.929,374.4902;Inherit;False;2;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;253;-2155.913,120.8101;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;256;-1406.124,509.1302;Half;False;Property;_VerTexPannerY1;    偏移V方向速度;44;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;254;-1288.112,-293.5814;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;246;-1589.405,950.8767;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;262;-1406.665,261.0807;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;257;-1530.621,-968.5852;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;263;-1824.07,-180.8726;Inherit;False;Constant;_Float1;Float 1;28;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;266;-1225.713,456.5954;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;265;-1750.844,789.6364;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;259;-1438.212,23.6934;Inherit;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;258;-1133.04,-59.35567;Inherit;False;Property;_IsFresnel;IsFresnel;32;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;-1;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;264;-1952.285,-46.86098;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;260;-1918.6,184.7375;Inherit;False;Property;_Keyword0;Keyword 0;23;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;-1;True;True;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WireNode;261;-1647.859,-216.0324;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;271;-925.1506,2.321326;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;269;-1625.609,-51.18368;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PannerNode;268;-1069.871,265.6922;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;267;-1642.977,-181.5121;Inherit;False;Property;_IsDoubleFace1;开启双面异色;37;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;270;-1305.619,-995.8552;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;277;-1137.657,-197.1077;Inherit;False;Property;_IsFresnel2;开启Fresnel;33;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;274;-889.1497,236.9384;Inherit;True;Property;_VerTex1;    顶点偏移图;41;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;275;-761.3126,155.2856;Half;False;Property;_VerTexScale1;    偏移强度;42;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;272;-1438.48,-79.03518;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;273;-853.1548,79.84182;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;276;-767.9098,440.1995;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;278;-559.429,243.5182;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;279;-731.6156,-42.57522;Inherit;False;Property;_Fresnel3;开启Fresnel;34;0;Create;False;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;2;Add;Multiply;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;281;-565.4858,154.2172;Inherit;False;Constant;_Float2;Float 2;29;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;280;-920.2378,-97.65768;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;150;-2545.801,-677.5261;Inherit;False;699.0938;111.7469;Shader模式;5;288;286;284;283;282;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;283;-2247.647,-643.4802;Inherit;False;Property;_CullMode;Cull Mode;2;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;286;-2112.421,-645.7249;Inherit;False;Property;_ZWrite;ZWrite;3;1;[Enum];Create;True;0;2;On;1;Off;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;288;-1985.384,-643.8282;Inherit;False;Property;_ZTest;ZTest;4;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;285;-418.2019,154.0119;Inherit;False;Property;_IsVertexOffset1;开启顶点偏移;40;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;287;-498.8259,-109.0791;Inherit;True;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;284;-2514.029,-644.9008;Inherit;False;Property;_Src;SrcBlend;0;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;282;-2383.196,-644.6383;Inherit;False;Property;_Dst;DstBlend;1;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;-110.5937,-14.07556;Float;False;True;-1;2;ASEMaterialInspector;100;1;copyShader;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;2;5;True;284;10;True;282;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;0;True;283;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;True;1;True;286;True;3;True;288;True;True;0;False;-1;0;False;-1;True;3;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;PreviewType=Plane;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;154;0;152;0
WireConnection;154;1;151;0
WireConnection;155;0;153;0
WireConnection;155;2;154;0
WireConnection;156;1;155;0
WireConnection;157;0;156;1
WireConnection;157;1;156;4
WireConnection;159;0;294;3
WireConnection;160;0;157;0
WireConnection;161;0;158;0
WireConnection;161;1;160;0
WireConnection;161;2;159;0
WireConnection;289;0;161;0
WireConnection;164;0;293;1
WireConnection;164;1;161;0
WireConnection;165;0;162;1
WireConnection;165;1;163;0
WireConnection;167;0;161;0
WireConnection;167;1;293;2
WireConnection;166;0;162;2
WireConnection;166;1;163;0
WireConnection;171;0;165;0
WireConnection;171;1;162;2
WireConnection;174;0;164;0
WireConnection;174;1;293;2
WireConnection;172;0;162;1
WireConnection;172;1;166;0
WireConnection;170;0;162;0
WireConnection;170;1;163;0
WireConnection;169;0;161;0
WireConnection;169;1;293;0
WireConnection;168;0;293;1
WireConnection;168;1;167;0
WireConnection;176;0;175;0
WireConnection;176;1;173;0
WireConnection;178;1;170;0
WireConnection;178;0;171;0
WireConnection;178;2;172;0
WireConnection;290;1;169;0
WireConnection;290;0;174;0
WireConnection;290;2;168;0
WireConnection;182;0;290;0
WireConnection;185;0;177;0
WireConnection;187;0;178;0
WireConnection;187;2;176;0
WireConnection;186;0;294;1
WireConnection;186;1;294;2
WireConnection;189;1;187;0
WireConnection;190;1;183;0
WireConnection;191;0;180;2
WireConnection;191;1;188;0
WireConnection;192;1;179;4
WireConnection;192;0;181;0
WireConnection;196;1;186;0
WireConnection;196;0;184;0
WireConnection;197;0;180;1
WireConnection;197;1;188;0
WireConnection;198;0;182;0
WireConnection;206;0;193;0
WireConnection;206;1;195;0
WireConnection;200;0;188;0
WireConnection;200;1;180;0
WireConnection;201;0;198;0
WireConnection;201;1;196;0
WireConnection;202;0;180;1
WireConnection;202;1;191;0
WireConnection;203;0;197;0
WireConnection;203;1;180;2
WireConnection;207;0;189;1
WireConnection;204;0;194;0
WireConnection;208;0;192;0
WireConnection;208;1;190;0
WireConnection;205;0;199;0
WireConnection;211;0;210;0
WireConnection;211;1;209;0
WireConnection;212;1;200;0
WireConnection;212;0;203;0
WireConnection;212;2;202;0
WireConnection;213;1;204;0
WireConnection;214;1;205;0
WireConnection;215;0;208;0
WireConnection;215;1;183;0
WireConnection;216;1;189;1
WireConnection;216;0;207;0
WireConnection;292;0;201;0
WireConnection;292;2;206;0
WireConnection;220;0;214;0
WireConnection;220;1;192;0
WireConnection;218;0;215;0
WireConnection;218;1;213;0
WireConnection;291;1;292;0
WireConnection;221;0;216;0
WireConnection;223;0;212;0
WireConnection;223;2;211;0
WireConnection;217;0;216;0
WireConnection;224;0;291;0
WireConnection;225;0;221;0
WireConnection;225;1;220;0
WireConnection;228;0;217;0
WireConnection;228;1;218;0
WireConnection;229;0;222;0
WireConnection;230;1;223;0
WireConnection;227;3;219;0
WireConnection;232;1;291;4
WireConnection;232;0;291;1
WireConnection;235;0;225;0
WireConnection;235;1;177;0
WireConnection;236;0;230;1
WireConnection;236;1;230;4
WireConnection;238;0;228;0
WireConnection;238;1;177;0
WireConnection;237;0;226;0
WireConnection;237;1;231;0
WireConnection;237;2;229;0
WireConnection;240;0;227;0
WireConnection;233;1;291;0
WireConnection;233;0;224;0
WireConnection;247;0;239;4
WireConnection;247;1;240;0
WireConnection;247;2;242;4
WireConnection;252;0;238;0
WireConnection;252;1;235;0
WireConnection;245;0;236;0
WireConnection;255;0;241;4
WireConnection;248;0;232;0
WireConnection;250;0;241;0
WireConnection;250;1;233;0
WireConnection;250;2;234;0
WireConnection;253;0;234;4
WireConnection;254;0;237;0
WireConnection;246;0;238;0
WireConnection;257;0;227;0
WireConnection;266;0;244;0
WireConnection;266;1;256;0
WireConnection;265;0;252;0
WireConnection;259;0;255;0
WireConnection;259;1;248;0
WireConnection;259;2;253;0
WireConnection;259;3;245;0
WireConnection;259;4;246;0
WireConnection;258;1;251;0
WireConnection;258;0;247;0
WireConnection;264;0;250;0
WireConnection;260;1;243;0
WireConnection;260;0;249;0
WireConnection;261;0;254;0
WireConnection;271;0;258;0
WireConnection;271;1;259;0
WireConnection;269;0;264;0
WireConnection;269;1;260;0
WireConnection;269;2;265;0
WireConnection;268;0;262;0
WireConnection;268;2;266;0
WireConnection;267;1;263;0
WireConnection;267;0;261;0
WireConnection;270;0;239;0
WireConnection;270;1;257;0
WireConnection;277;1;251;0
WireConnection;277;0;270;0
WireConnection;274;1;268;0
WireConnection;272;0;267;0
WireConnection;272;1;269;0
WireConnection;273;0;271;0
WireConnection;273;1;259;0
WireConnection;278;0;275;0
WireConnection;278;1;274;1
WireConnection;278;2;276;0
WireConnection;279;1;271;0
WireConnection;279;0;273;0
WireConnection;280;0;277;0
WireConnection;280;1;272;0
WireConnection;285;1;281;0
WireConnection;285;0;278;0
WireConnection;287;0;280;0
WireConnection;287;3;279;0
WireConnection;0;0;287;0
WireConnection;0;1;285;0
ASEEND*/
//CHKSM=065749C70540D367767C23219AB03D09949F039A