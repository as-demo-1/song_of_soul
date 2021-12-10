Shader "VFX/ParticleDissolveSlash" {
    Properties {
        _Textures ("Textures", 2D) = "white" {}
        [HDR]_diffuse_color ("diffuse_color", Color) = (0.5,0.5,0.5,1)
        [MaterialToggle] _dissolve ("dissolve", Float ) = 1
        _U_speed ("U_speed", Float ) = 0
        _V_speed ("V_speed", Float ) = 0
        _Dissolve_Tex ("Dissolve_Tex", 2D) = "white" {}
        _OpacityTex ("OpacityTex", 2D) = "white" {}
        [MaterialToggle] _UV_on ("UV_on", Float ) = 0
        [MaterialToggle] _alpha ("alpha", Float ) = 0
        [MaterialToggle] _Addswitch ("Addswitch", Float ) = 0
        _mask_U ("mask_U", Float ) = 0
        _mask_V ("mask_V", Float ) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5

        _Stencil("Stencil ID", Float) = 0
        _StencilComp("Stencil Comparison", Float) = 8
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
    }

    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

		Stencil{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ColorMask RGB
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles3 metal 
            #pragma target 3.0
            uniform sampler2D _Textures; uniform float4 _Textures_ST;
            uniform float4 _diffuse_color;
            uniform fixed _dissolve;
            uniform float _U_speed;
            uniform float _V_speed;
            uniform sampler2D _Dissolve_Tex; uniform float4 _Dissolve_Tex_ST;
            uniform sampler2D _OpacityTex; uniform float4 _OpacityTex_ST;
            uniform fixed _UV_on;
            uniform fixed _alpha;
            uniform fixed _Addswitch;
            uniform float _mask_U;
            uniform float _mask_V;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 node_797 = _Time;
                float2 _UV_on_var = lerp( (i.uv0+float2((_U_speed*node_797.g),(node_797.g*_V_speed))), (i.uv0+float2(i.uv1.b,i.uv1.a)), _UV_on );
                float4 _Textures_var = tex2D(_Textures,TRANSFORM_TEX(_UV_on_var, _Textures));
                float _alpha_var = lerp( _Textures_var.r, _Textures_var.a, _alpha );
                float3 emissive = (float4(_Textures_var.rgb,_alpha_var)*_diffuse_color.rgb*i.vertexColor.rgb).rgb;
                float3 finalColor = emissive;
                float4 _Dissolve_Tex_var = tex2D(_Dissolve_Tex,TRANSFORM_TEX(i.uv0, _Dissolve_Tex));
                float _dissolve_var = lerp( 1.0, step(i.uv1.r,_Dissolve_Tex_var.r), _dissolve );
                float4 node_8156 = _Time;
                float2 node_7325 = (i.uv0+float2((_mask_U*node_8156.g),(node_8156.g*_mask_V)));
                float4 _OpacityTex_var = tex2D(_OpacityTex,TRANSFORM_TEX(node_7325, _OpacityTex));
                return fixed4(finalColor,lerp( (_alpha_var*_dissolve_var*i.vertexColor.a*_OpacityTex_var.r*_diffuse_color.a), (_alpha_var*((_alpha_var*i.vertexColor.a)+_OpacityTex_var.r)*_dissolve_var), _Addswitch ));
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Off
            ColorMask RGB
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 glcore gles3 metal 
            #pragma target 3.0
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
   // CustomEditor "ShaderForgeMaterialInspector"
}
