// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CXYL/第三阶刀光1"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.BlendMode)]_Src("SrcBlend", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)]_Dst("DstBlend", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Float) = 0
		[Enum(On,1,Off,0)]_ZWrite("ZWrite", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Float) = 4
		[HDR]_Color("主贴图颜色", Color) = (1,1,1,1)
		_MainTex("主帖图", 2D) = "white" {}
		[Toggle(_DESATURATE_ON)] _Desaturate("去色", Float) = 0
		[Toggle(_DEBLACKBG_ON)] _DeBlackBG("去黑", Float) = 0
		_MainTexPannerX("主帖图U方向速度", Float) = 0
		_MainTexPannerY("主帖图V方向速度", Float) = 0
		_TurbulenceTex("扭曲贴图", 2D) = "white" {}
		_TurbulenceTexPannerX("    扭曲图U方向速度", Float) = 0
		_TurbulenceTexPannerY("    扭曲图V方向速度", Float) = 0
		[KeywordEnum(UV,U,V)] _TurbulenceDirection("    扭曲方向", Float) = 0
		_TurbulenceStrength("    扭曲强度(Custom1.z)", Float) = 0
		_MaskTex("遮罩图", 2D) = "white" {}
		_MaskTexPannerX("    遮罩图U方向速度", Float) = 0
		_MaskTexPannerY("    遮罩图V方向速度", Float) = 0
		_DissolveTex("溶解图", 2D) = "white" {}
		[KeywordEnum(Particle,Material)] _DissolveMode("    溶解模式", Float) = 1
		[Toggle(_ONEMINUS_DISSOLVETEX_ON)] _OneMinus_DissolveTex("    溶解图反向", Float) = 0
		_DissolveTexPannerX("    溶解图U方向速度", Float) = 0
		_DissolveTexPannerY("    溶解图V方向速度", Float) = 0
		_Dissolve("    溶解值(Custom1.w)", Range( 0 , 1)) = 0
		_Hardness("    硬度", Range( 0 , 0.99)) = 0
		[HDR]_EdgeColor("    描边颜色(Custom2.color)", Color) = (1,1,1,1)
		_EdgeWidth("    描边宽度", Range( 0 , 1)) = 0
		[Toggle(_ISFRESNEL_ON)] _IsFresnel("开启Fresnel", Float) = 0
		[KeywordEnum(Add,Multiply)] _Fresnel2("开启Fresnel", Float) = 0
		[HDR]_FresnelColor("    Fresnel颜色", Color) = (1,1,1,1)
		_FresnelScale("    Fresnel强度", Float) = 1
		[Toggle(_ISDOUBLEFACE_ON)] _IsDoubleFace("开启双面异色", Float) = 0
		[HDR]_FaceInColor("    内面颜色", Color) = (1,1,1,1)
		[HDR]_FaceOutColor("    外面颜色", Color) = (1,1,1,1)
		[Toggle(_ISVERTEXOFFSET_ON)] _IsVertexOffset("开启顶点偏移", Float) = 0
		_VerTex("    顶点偏移图", 2D) = "white" {}
		_VerTexScale("    偏移强度", Float) = 0
		_VerTexPannerX("    偏移U方向速度", Float) = 0
		_VerTexPannerY("    偏移V方向速度", Float) = 0

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" "Queue"="Transparent" "PreviewType"="Plane" }
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
			#pragma shader_feature_local _ISVERTEXOFFSET_ON
			#pragma shader_feature_local _ISFRESNEL_ON
			#pragma shader_feature_local _ISDOUBLEFACE_ON
			#pragma shader_feature_local _DESATURATE_ON
			#pragma shader_feature_local _TURBULENCEDIRECTION_UV _TURBULENCEDIRECTION_U _TURBULENCEDIRECTION_V
			#pragma shader_feature_local _DISSOLVEMODE_PARTICLE _DISSOLVEMODE_MATERIAL
			#pragma shader_feature_local _ONEMINUS_DISSOLVETEX_ON
			#pragma shader_feature_local _FRESNEL2_ADD _FRESNEL2_MULTIPLY
			#pragma shader_feature_local _DEBLACKBG_ON


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				half3 ase_normal : NORMAL;
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

			uniform half _ZWrite;
			uniform half _Src;
			uniform half _CullMode;
			uniform half _Dst;
			uniform half _ZTest;
			uniform half _VerTexScale;
			uniform sampler2D _VerTex;
			uniform half _VerTexPannerX;
			uniform half _VerTexPannerY;
			uniform half4 _VerTex_ST;
			uniform half4 _FresnelColor;
			uniform half _FresnelScale;
			uniform half4 _FaceInColor;
			uniform half4 _FaceOutColor;
			uniform half4 _Color;
			uniform sampler2D _MainTex;
			uniform half _MainTexPannerX;
			uniform half _MainTexPannerY;
			uniform half _TurbulenceStrength;
			uniform sampler2D _TurbulenceTex;
			uniform half _TurbulenceTexPannerX;
			uniform half _TurbulenceTexPannerY;
			uniform half4 _TurbulenceTex_ST;
			uniform half4 _MainTex_ST;
			uniform half4 _EdgeColor;
			uniform half _Hardness;
			uniform sampler2D _DissolveTex;
			uniform half _DissolveTexPannerX;
			uniform half _DissolveTexPannerY;
			uniform half4 _DissolveTex_ST;
			uniform half _Dissolve;
			uniform half _EdgeWidth;
			uniform sampler2D _MaskTex;
			uniform half _MaskTexPannerX;
			uniform half _MaskTexPannerY;
			uniform half4 _MaskTex_ST;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				half3 temp_cast_0 = (0.0).xxx;
				half2 appendResult124 = (half2(_VerTexPannerX , _VerTexPannerY));
				half2 uv_VerTex = v.ase_texcoord.xy * _VerTex_ST.xy + _VerTex_ST.zw;
				half2 panner121 = ( 1.0 * _Time.y * appendResult124 + uv_VerTex);
				#ifdef _ISVERTEXOFFSET_ON
				half3 staticSwitch128 = ( _VerTexScale * tex2Dlod( _VerTex, float4( panner121, 0, 0.0) ).r * v.ase_normal );
				#else
				half3 staticSwitch128 = temp_cast_0;
				#endif
				
				half3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
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
				vertexValue = staticSwitch128;
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
			
			fixed4 frag (v2f i , half ase_vface : VFACE) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				half4 temp_cast_0 = (0.0).xxxx;
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(WorldPosition);
				ase_worldViewDir = normalize(ase_worldViewDir);
				half3 ase_worldNormal = i.ase_texcoord1.xyz;
				half fresnelNdotV91 = dot( ase_worldNormal, ase_worldViewDir );
				half fresnelNode91 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV91, _FresnelScale ) );
				#ifdef _ISFRESNEL_ON
				half4 staticSwitch104 = ( _FresnelColor * saturate( fresnelNode91 ) );
				#else
				half4 staticSwitch104 = temp_cast_0;
				#endif
				half4 temp_cast_1 = (1.0).xxxx;
				half4 lerpResult90 = lerp( _FaceInColor , _FaceOutColor , (ase_vface*0.5 + 0.5));
				#ifdef _ISDOUBLEFACE_ON
				half4 staticSwitch102 = lerpResult90;
				#else
				half4 staticSwitch102 = temp_cast_1;
				#endif
				half2 appendResult25 = (half2(_MainTexPannerX , _MainTexPannerY));
				half2 appendResult35 = (half2(_TurbulenceTexPannerX , _TurbulenceTexPannerY));
				half2 uv_TurbulenceTex = i.ase_texcoord2.xy * _TurbulenceTex_ST.xy + _TurbulenceTex_ST.zw;
				half2 panner37 = ( 1.0 * _Time.y * appendResult35 + uv_TurbulenceTex);
				half4 tex2DNode26 = tex2D( _TurbulenceTex, panner37 );
				half4 texCoord78 = i.ase_texcoord3;
				texCoord78.xy = i.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				half temp_output_28_0 = ( _TurbulenceStrength * ( ( tex2DNode26.r * tex2DNode26.a ) - 0.5 ) * ( 1.0 - texCoord78.z ) );
				half2 uv_MainTex = i.ase_texcoord2.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				half2 appendResult154 = (half2(( uv_MainTex.x + temp_output_28_0 ) , uv_MainTex.y));
				half2 appendResult151 = (half2(uv_MainTex.x , ( temp_output_28_0 + uv_MainTex.y )));
				#if defined(_TURBULENCEDIRECTION_UV)
				half2 staticSwitch152 = ( temp_output_28_0 + uv_MainTex );
				#elif defined(_TURBULENCEDIRECTION_U)
				half2 staticSwitch152 = appendResult154;
				#elif defined(_TURBULENCEDIRECTION_V)
				half2 staticSwitch152 = appendResult151;
				#else
				half2 staticSwitch152 = ( temp_output_28_0 + uv_MainTex );
				#endif
				half2 appendResult79 = (half2(texCoord78.x , texCoord78.y));
				#if defined(_DISSOLVEMODE_PARTICLE)
				half2 staticSwitch187 = appendResult79;
				#elif defined(_DISSOLVEMODE_MATERIAL)
				half2 staticSwitch187 = half2( 0,0 );
				#else
				half2 staticSwitch187 = half2( 0,0 );
				#endif
				half2 panner21 = ( 1.0 * _Time.y * appendResult25 + ( staticSwitch152 + staticSwitch187 ));
				half4 tex2DNode1 = tex2D( _MainTex, panner21 );
				half3 desaturateInitialColor131 = tex2DNode1.rgb;
				half desaturateDot131 = dot( desaturateInitialColor131, float3( 0.299, 0.587, 0.114 ));
				half3 desaturateVar131 = lerp( desaturateInitialColor131, desaturateDot131.xxx, 1.0 );
				#ifdef _DESATURATE_ON
				half4 staticSwitch133 = half4( desaturateVar131 , 0.0 );
				#else
				half4 staticSwitch133 = tex2DNode1;
				#endif
				half4 texCoord80 = i.ase_texcoord4;
				texCoord80.xy = i.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				#if defined(_DISSOLVEMODE_PARTICLE)
				half4 staticSwitch186 = texCoord80;
				#elif defined(_DISSOLVEMODE_MATERIAL)
				half4 staticSwitch186 = _EdgeColor;
				#else
				half4 staticSwitch186 = _EdgeColor;
				#endif
				half2 appendResult175 = (half2(_DissolveTexPannerX , _DissolveTexPannerY));
				half2 uv_DissolveTex = i.ase_texcoord2.xy * _DissolveTex_ST.xy + _DissolveTex_ST.zw;
				half Turbulence161 = temp_output_28_0;
				half2 appendResult169 = (half2(( uv_DissolveTex.x + Turbulence161 ) , uv_DissolveTex.y));
				half2 appendResult170 = (half2(uv_DissolveTex.x , ( uv_DissolveTex.y + Turbulence161 )));
				#if defined(_TURBULENCEDIRECTION_UV)
				half2 staticSwitch164 = ( uv_DissolveTex + Turbulence161 );
				#elif defined(_TURBULENCEDIRECTION_U)
				half2 staticSwitch164 = appendResult169;
				#elif defined(_TURBULENCEDIRECTION_V)
				half2 staticSwitch164 = appendResult170;
				#else
				half2 staticSwitch164 = ( uv_DissolveTex + Turbulence161 );
				#endif
				half2 panner172 = ( 1.0 * _Time.y * appendResult175 + staticSwitch164);
				half4 tex2DNode64 = tex2D( _DissolveTex, panner172 );
				#ifdef _ONEMINUS_DISSOLVETEX_ON
				half staticSwitch149 = ( 1.0 - tex2DNode64.r );
				#else
				half staticSwitch149 = tex2DNode64.r;
				#endif
				half4 texCoord74 = i.ase_texcoord3;
				texCoord74.xy = i.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				#if defined(_DISSOLVEMODE_PARTICLE)
				half staticSwitch185 = texCoord74.w;
				#elif defined(_DISSOLVEMODE_MATERIAL)
				half staticSwitch185 = _Dissolve;
				#else
				half staticSwitch185 = _Dissolve;
				#endif
				half Hardness51 = _Hardness;
				half smoothstepResult73 = smoothstep( _Hardness , 1.0 , ( ( staticSwitch149 + 1.0 ) - ( ( ( staticSwitch185 * ( 1.0 + _EdgeWidth ) ) - _EdgeWidth ) * ( 1.0 + ( 1.0 - Hardness51 ) ) ) ));
				half smoothstepResult72 = smoothstep( _Hardness , 1.0 , ( ( staticSwitch149 + 1.0 ) - ( ( 1.0 + ( 1.0 - Hardness51 ) ) * staticSwitch185 ) ));
				half4 lerpResult76 = lerp( ( _Color * staticSwitch133 * i.ase_color ) , staticSwitch186 , ( smoothstepResult73 - smoothstepResult72 ));
				#ifdef _ISFRESNEL_ON
				half staticSwitch146 = ( _FresnelColor.a * saturate( fresnelNode91 ) * i.ase_color.a );
				#else
				half staticSwitch146 = 0.0;
				#endif
				#ifdef _DEBLACKBG_ON
				half staticSwitch18 = tex2DNode1.r;
				#else
				half staticSwitch18 = tex2DNode1.a;
				#endif
				half2 appendResult47 = (half2(_MaskTexPannerX , _MaskTexPannerY));
				half2 uv_MaskTex = i.ase_texcoord2.xy * _MaskTex_ST.xy + _MaskTex_ST.zw;
				half2 appendResult178 = (half2(( uv_MaskTex.x + Turbulence161 ) , uv_MaskTex.y));
				half2 appendResult177 = (half2(uv_MaskTex.x , ( uv_MaskTex.y + Turbulence161 )));
				#if defined(_TURBULENCEDIRECTION_UV)
				half2 staticSwitch181 = ( Turbulence161 + uv_MaskTex );
				#elif defined(_TURBULENCEDIRECTION_U)
				half2 staticSwitch181 = appendResult178;
				#elif defined(_TURBULENCEDIRECTION_V)
				half2 staticSwitch181 = appendResult177;
				#else
				half2 staticSwitch181 = ( Turbulence161 + uv_MaskTex );
				#endif
				half2 panner48 = ( 1.0 * _Time.y * appendResult47 + staticSwitch181);
				half4 tex2DNode43 = tex2D( _MaskTex, panner48 );
				half temp_output_16_0 = ( _Color.a * staticSwitch18 * i.ase_color.a * ( tex2DNode43.r * tex2DNode43.a ) * smoothstepResult73 );
				half temp_output_143_0 = ( staticSwitch146 + temp_output_16_0 );
				#if defined(_FRESNEL2_ADD)
				half staticSwitch189 = temp_output_143_0;
				#elif defined(_FRESNEL2_MULTIPLY)
				half staticSwitch189 = ( temp_output_143_0 * temp_output_16_0 );
				#else
				half staticSwitch189 = temp_output_143_0;
				#endif
				half4 appendResult17 = (half4(( staticSwitch104 + ( staticSwitch102 * lerpResult76 ) ).rgb , staticSwitch189));
				
				
				finalColor = appendResult17;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18900
780;334.4;1523;468;1797.312;778.8746;4.059518;True;False
Node;AmplifyShaderEditor.CommentaryNode;136;-2967.43,-598.7254;Inherit;False;2079.259;652.5637;扭曲+UV滚动;27;21;27;25;152;23;151;154;155;79;150;153;22;28;78;29;33;32;26;37;35;38;36;34;156;157;184;187;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-2957.802,-308.5831;Inherit;False;Property;_TurbulenceTexPannerY;    扭曲图V方向速度;13;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-2958.802,-407.5831;Inherit;False;Property;_TurbulenceTexPannerX;    扭曲图U方向速度;12;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;38;-2795.381,-553.653;Inherit;False;0;26;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;35;-2717.323,-385.9977;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;37;-2548.843,-487.5911;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;26;-2355.342,-515.953;Inherit;True;Property;_TurbulenceTex;扭曲贴图;11;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-2057.045,-461.7054;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;78;-2220.554,-223.2564;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;184;-1971.967,-349.1763;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;33;-1887.719,-460.8234;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-1970.771,-569.1664;Inherit;False;Property;_TurbulenceStrength;    扭曲强度(Custom1.z);15;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-1733.02,-484.5875;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;161;-1562.842,-681.6442;Half;False;Turbulence;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;22;-1824.37,-332.0011;Inherit;False;0;1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;163;-2821.21,459.875;Inherit;False;0;64;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;162;-2786.772,380.7934;Inherit;False;161;Turbulence;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;150;-1560.201,-394.3706;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;166;-2539.968,445.6442;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;165;-2537.401,342.0307;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;153;-1558.878,-507.5262;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;151;-1393,-416.9977;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;167;-2537.807,537.0749;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;174;-2324.521,568.4049;Inherit;False;Property;_DissolveTexPannerX;    溶解图U方向速度;22;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;155;-1392.726,-322.1721;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;135;-1915.628,552.7122;Inherit;False;1906.367;735.2061;溶解;27;74;54;77;73;72;70;71;69;66;67;68;63;65;62;59;61;60;56;57;58;52;51;50;64;148;149;185;;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;170;-2353.924,437.8023;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;154;-1395.991,-511.6037;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;169;-2352.456,342.5704;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;173;-2320.997,661.3575;Inherit;False;Property;_DissolveTexPannerY;    溶解图V方向速度;23;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;152;-1239.323,-485.5921;Inherit;False;Property;_TurbulenceDirection;    扭曲方向;14;0;Create;False;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;UV;U;V;Create;True;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;175;-2112.277,574.171;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-785.041,824.4155;Half;False;Property;_Hardness;    硬度;25;0;Create;False;0;0;0;False;0;False;0;0;0;0.99;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;164;-2175.257,384.5943;Inherit;False;Property;_TurbulenceDirection;    TurbulenceDirection;14;0;Create;True;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;UV;U;V;Reference;152;True;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;182;-1778.632,145.4394;Inherit;False;161;Turbulence;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;156;-1016.824,-368.0939;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;52;-1563.568,1020.591;Half;False;Property;_EdgeWidth;    描边宽度;27;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;188;-1596.543,-56.01715;Inherit;False;Constant;_Vector0;Vector 0;40;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;54;-1906.156,920.6826;Half;False;Property;_Dissolve;    溶解值(Custom1.w);24;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;51;-449.825,824.0595;Half;False;Hardness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;79;-1733.834,-196.8185;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;74;-1849.106,732.7979;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;180;-1810.118,222.7425;Inherit;False;0;43;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;172;-1863.594,388.5508;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-1421.338,-80.47673;Inherit;False;Property;_MainTexPannerX;主帖图U方向速度;9;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;64;-1680.934,577.9186;Inherit;True;Property;_DissolveTex;溶解图;19;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;185;-1606.352,813.1472;Inherit;False;Property;_DissolveMode;    溶解模式;20;0;Create;False;0;0;0;False;0;False;0;1;1;True;;KeywordEnum;2;Particle;Material;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;56;-1424.592,764.3737;Inherit;False;51;Hardness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;157;-1209.627,-358.228;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;58;-1293.187,953.418;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;57;-1423.501,1214.629;Inherit;False;51;Hardness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-1417.814,12.4759;Inherit;False;Property;_MainTexPannerY;主帖图V方向速度;10;0;Create;False;0;0;0;False;0;False;0;-1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;179;-1534.193,203.1942;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;187;-1407.543,-205.0172;Inherit;False;Property;_Keyword1;Keyword 1;20;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;185;True;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;176;-1531.627,99.58079;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;177;-1357.872,193.1918;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-1265.433,325.0031;Inherit;False;Property;_MaskTexPannerX;    遮罩图U方向速度;17;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;183;-1532.1,295.4081;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;27;-1178.374,-322.6332;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;46;-1261.704,408.5708;Inherit;False;Property;_MaskTexPannerY;    遮罩图V方向速度;18;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;178;-1356.404,100.1205;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;61;-1248.732,1219.846;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-1164.187,894.4182;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;60;-1249.822,769.5905;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;25;-1237.694,-76.01067;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;148;-1369.512,669.1481;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;21;-1051.993,-204.0398;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;149;-1215.573,602.8107;Inherit;False;Property;_OneMinus_DissolveTex;    溶解图反向;21;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;62;-1056.137,1008.938;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;63;-1091.957,745.8474;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;65;-1092.417,1196.102;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;181;-1174.235,141.0551;Inherit;False;Property;_TurbulenceDirection;    TurbulenceDirection;14;0;Create;True;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;UV;U;V;Reference;164;True;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;47;-1045.488,330.608;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;95;-63.47663,-957.8495;Inherit;False;Property;_FresnelScale;    Fresnel强度;31;0;Create;False;0;0;0;False;0;False;1;0.97;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;48;-859.7875,202.5789;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;67;-951.3931,747.0666;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;69;-888.2178,900.1616;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-889.3976,1173.681;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;68;-902.2077,1012.697;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-882.2373,-231.725;Inherit;True;Property;_MainTex;主帖图;6;0;Create;False;0;0;0;False;0;False;-1;None;a2265baf34307624f9f55d7fc8da7aa7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FaceVariableNode;86;246.8511,-432.3516;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;91;119.8255,-1049.852;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;70;-715.0331,911.7709;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;71;-711.4496,1016.211;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;85;356.0255,-431.6897;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;89;275.6373,-817.5563;Inherit;False;Property;_FaceInColor;    内面颜色;33;1;[HDR];Create;False;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DesaturateOpNode;131;-536.563,-208.0831;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;160;277.399,-601.7878;Inherit;False;Property;_FaceOutColor;    外面颜色;34;1;[HDR];Create;False;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;43;-663.7909,174.5313;Inherit;True;Property;_MaskTex;遮罩图;16;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;73;-472.373,987.9485;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;99;412.5772,-1290.424;Half;False;Property;_FresnelColor;    Fresnel颜色;30;1;[HDR];Create;False;0;0;0;False;0;False;1,1,1,1;0.9811321,0.9765445,0.5877537,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;159;484.68,-1119.983;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-369.2852,225.4668;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;90;573.0758,-620.8551;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;147;457.3853,-1482.216;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;72;-477.7257,589.7831;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;133;-354.7343,-236.7389;Inherit;False;Property;_Desaturate;去色;7;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;4;-365.694,-432.7251;Inherit;False;Property;_Color;主贴图颜色;5;1;[HDR];Create;False;0;0;0;False;0;False;1,1,1,1;1.432495,0.2432538,0.0180188,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;11;-381.0239,13.77601;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;18;-428.4046,-107.4474;Inherit;False;Property;_DeBlackBG;去黑;8;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;109;-139.4301,-78.25585;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;118;725.1888,-371.9792;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;113;-139.3716,77.80909;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;107;423.8957,872.4789;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;145;710.212,-1183.719;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;112;-140.8361,-14.28275;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;105;664.9247,-274.8725;Inherit;False;Constant;_Float1;Float 1;29;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;80;-152.6282,296.0924;Inherit;False;2;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-104.4086,-255.3602;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;77;-227.2683,765.5526;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;111;-142.6121,42.4123;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;122;607.8053,348.4756;Half;False;Property;_VerTexPannerX;    偏移U方向速度;38;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;123;607.1765,430.7324;Half;False;Property;_VerTexPannerY;    偏移V方向速度;39;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;75;-157.8298,128.077;Half;False;Property;_EdgeColor;    描边颜色(Custom2.color);26;1;[HDR];Create;False;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;146;880.2609,-137.7535;Inherit;False;Property;_IsFresnel;IsFresnel;28;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;104;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;575.0891,-54.70442;Inherit;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;186;94.70081,106.3397;Inherit;False;Property;_Keyword0;Keyword 0;20;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;185;True;True;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WireNode;119;365.4419,-294.4302;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;158;482.68,-1046.983;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;103;189.2305,-259.2704;Inherit;False;Constant;_Float0;Float 0;28;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;116;61.01605,-125.2588;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;108;262.4565,711.2386;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;106;606.6354,182.6829;Inherit;False;0;120;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;124;787.5874,378.1976;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;102;370.3238,-259.9099;Inherit;False;Property;_IsDoubleFace;开启双面异色;32;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;707.6819,-1074.253;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;143;1088.15,-76.07649;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;121;943.4298,187.2944;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;76;387.6914,-129.5815;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;104;875.6436,-275.5055;Inherit;False;Property;_IsFresnel;开启Fresnel;28;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalVertexDataNode;125;1245.391,361.8017;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;101;574.821,-157.433;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;120;1124.151,158.5406;Inherit;True;Property;_VerTex;    顶点偏移图;36;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;190;1160.146,1.444008;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;129;1251.988,76.88779;Half;False;Property;_VerTexScale;    偏移强度;37;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;94;1093.063,-176.0555;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;126;1453.872,165.1204;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;137;-532.4995,-755.9239;Inherit;False;699.0938;111.7469;Shader模式;5;6;7;10;8;9;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;127;1447.815,75.81943;Inherit;False;Constant;_Float2;Float 2;29;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;189;1281.685,-120.973;Inherit;False;Property;_Fresnel2;开启Fresnel;29;0;Create;False;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;2;Add;Multiply;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;27.9172,-722.226;Inherit;False;Property;_ZTest;ZTest;4;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;17;1514.475,-187.4769;Inherit;True;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-99.12,-724.1226;Inherit;False;Property;_ZWrite;ZWrite;3;1;[Enum];Create;True;0;2;On;1;Off;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;128;1595.099,75.6141;Inherit;False;Property;_IsVertexOffset;开启顶点偏移;35;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-499.3593,-723.2985;Inherit;False;Property;_Src;SrcBlend;0;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-234.3463,-721.8779;Inherit;False;Property;_CullMode;Cull Mode;2;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-367.1583,-722.2274;Inherit;False;Property;_Dst;DstBlend;1;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1872.06,-172.8258;Half;False;True;-1;2;ASEMaterialInspector;100;1;CXYL/第三阶刀光1;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;2;5;True;6;10;True;7;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;2;True;8;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;True;1;True;9;True;3;True;10;True;True;0;False;-1;0;False;-1;True;3;RenderType=Opaque=RenderType;Queue=Transparent=Queue=0;PreviewType=Plane;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;35;0;36;0
WireConnection;35;1;34;0
WireConnection;37;0;38;0
WireConnection;37;2;35;0
WireConnection;26;1;37;0
WireConnection;32;0;26;1
WireConnection;32;1;26;4
WireConnection;184;0;78;3
WireConnection;33;0;32;0
WireConnection;28;0;29;0
WireConnection;28;1;33;0
WireConnection;28;2;184;0
WireConnection;161;0;28;0
WireConnection;150;0;28;0
WireConnection;150;1;22;2
WireConnection;166;0;163;2
WireConnection;166;1;162;0
WireConnection;165;0;163;1
WireConnection;165;1;162;0
WireConnection;153;0;22;1
WireConnection;153;1;28;0
WireConnection;151;0;22;1
WireConnection;151;1;150;0
WireConnection;167;0;163;0
WireConnection;167;1;162;0
WireConnection;155;0;28;0
WireConnection;155;1;22;0
WireConnection;170;0;163;1
WireConnection;170;1;166;0
WireConnection;154;0;153;0
WireConnection;154;1;22;2
WireConnection;169;0;165;0
WireConnection;169;1;163;2
WireConnection;152;1;155;0
WireConnection;152;0;154;0
WireConnection;152;2;151;0
WireConnection;175;0;174;0
WireConnection;175;1;173;0
WireConnection;164;1;167;0
WireConnection;164;0;169;0
WireConnection;164;2;170;0
WireConnection;156;0;152;0
WireConnection;51;0;50;0
WireConnection;79;0;78;1
WireConnection;79;1;78;2
WireConnection;172;0;164;0
WireConnection;172;2;175;0
WireConnection;64;1;172;0
WireConnection;185;1;74;4
WireConnection;185;0;54;0
WireConnection;157;0;156;0
WireConnection;58;1;52;0
WireConnection;179;0;180;2
WireConnection;179;1;182;0
WireConnection;187;1;79;0
WireConnection;187;0;188;0
WireConnection;176;0;180;1
WireConnection;176;1;182;0
WireConnection;177;0;180;1
WireConnection;177;1;179;0
WireConnection;183;0;182;0
WireConnection;183;1;180;0
WireConnection;27;0;157;0
WireConnection;27;1;187;0
WireConnection;178;0;176;0
WireConnection;178;1;180;2
WireConnection;61;0;57;0
WireConnection;59;0;185;0
WireConnection;59;1;58;0
WireConnection;60;0;56;0
WireConnection;25;0;23;0
WireConnection;25;1;24;0
WireConnection;148;0;64;1
WireConnection;21;0;27;0
WireConnection;21;2;25;0
WireConnection;149;1;64;1
WireConnection;149;0;148;0
WireConnection;62;0;59;0
WireConnection;62;1;52;0
WireConnection;63;1;60;0
WireConnection;65;1;61;0
WireConnection;181;1;183;0
WireConnection;181;0;178;0
WireConnection;181;2;177;0
WireConnection;47;0;45;0
WireConnection;47;1;46;0
WireConnection;48;0;181;0
WireConnection;48;2;47;0
WireConnection;67;0;63;0
WireConnection;67;1;185;0
WireConnection;69;0;149;0
WireConnection;66;0;62;0
WireConnection;66;1;65;0
WireConnection;68;0;149;0
WireConnection;1;1;21;0
WireConnection;91;3;95;0
WireConnection;70;0;68;0
WireConnection;70;1;67;0
WireConnection;71;0;69;0
WireConnection;71;1;66;0
WireConnection;85;0;86;0
WireConnection;131;0;1;0
WireConnection;43;1;48;0
WireConnection;73;0;71;0
WireConnection;73;1;50;0
WireConnection;159;0;91;0
WireConnection;44;0;43;1
WireConnection;44;1;43;4
WireConnection;90;0;89;0
WireConnection;90;1;160;0
WireConnection;90;2;85;0
WireConnection;72;0;70;0
WireConnection;72;1;50;0
WireConnection;133;1;1;0
WireConnection;133;0;131;0
WireConnection;18;1;1;4
WireConnection;18;0;1;1
WireConnection;109;0;4;4
WireConnection;118;0;90;0
WireConnection;113;0;44;0
WireConnection;107;0;73;0
WireConnection;145;0;99;4
WireConnection;145;1;159;0
WireConnection;145;2;147;4
WireConnection;112;0;18;0
WireConnection;5;0;4;0
WireConnection;5;1;133;0
WireConnection;5;2;11;0
WireConnection;77;0;73;0
WireConnection;77;1;72;0
WireConnection;111;0;11;4
WireConnection;146;1;105;0
WireConnection;146;0;145;0
WireConnection;16;0;109;0
WireConnection;16;1;112;0
WireConnection;16;2;111;0
WireConnection;16;3;113;0
WireConnection;16;4;107;0
WireConnection;186;1;80;0
WireConnection;186;0;75;0
WireConnection;119;0;118;0
WireConnection;158;0;91;0
WireConnection;116;0;5;0
WireConnection;108;0;77;0
WireConnection;124;0;122;0
WireConnection;124;1;123;0
WireConnection;102;1;103;0
WireConnection;102;0;119;0
WireConnection;100;0;99;0
WireConnection;100;1;158;0
WireConnection;143;0;146;0
WireConnection;143;1;16;0
WireConnection;121;0;106;0
WireConnection;121;2;124;0
WireConnection;76;0;116;0
WireConnection;76;1;186;0
WireConnection;76;2;108;0
WireConnection;104;1;105;0
WireConnection;104;0;100;0
WireConnection;101;0;102;0
WireConnection;101;1;76;0
WireConnection;120;1;121;0
WireConnection;190;0;143;0
WireConnection;190;1;16;0
WireConnection;94;0;104;0
WireConnection;94;1;101;0
WireConnection;126;0;129;0
WireConnection;126;1;120;1
WireConnection;126;2;125;0
WireConnection;189;1;143;0
WireConnection;189;0;190;0
WireConnection;17;0;94;0
WireConnection;17;3;189;0
WireConnection;128;1;127;0
WireConnection;128;0;126;0
WireConnection;0;0;17;0
WireConnection;0;1;128;0
ASEEND*/
//CHKSM=A6D3778E58A2609FF1B0D34A4DE8370A787ECA62