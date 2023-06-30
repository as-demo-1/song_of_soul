// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:2768,x:32719,y:32712,varname:node_2768,prsc:2|normal-819-RGB,alpha-1480-OUT,refract-95-OUT;n:type:ShaderForge.SFN_Tex2d,id:819,x:32029,y:32911,ptovrint:False,ptlb:node_819,ptin:_node_819,varname:node_819,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_VertexColor,id:3116,x:32140,y:33084,varname:node_3116,prsc:2;n:type:ShaderForge.SFN_Vector1,id:144,x:32406,y:32578,varname:node_144,prsc:2,v1:1;n:type:ShaderForge.SFN_Multiply,id:95,x:32459,y:33042,varname:node_95,prsc:2|A-4512-OUT,B-3116-A,C-2419-OUT;n:type:ShaderForge.SFN_Append,id:4512,x:32221,y:32941,varname:node_4512,prsc:2|A-819-R,B-819-G;n:type:ShaderForge.SFN_Vector1,id:1480,x:32306,y:32794,varname:node_1480,prsc:2,v1:0;n:type:ShaderForge.SFN_TexCoord,id:4176,x:31516,y:33224,varname:node_4176,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_RemapRange,id:6319,x:31676,y:33240,varname:node_6319,prsc:2,frmn:0,frmx:1,tomn:-1,tomx:1|IN-4176-UVOUT;n:type:ShaderForge.SFN_Length,id:4998,x:31877,y:33268,varname:node_4998,prsc:2|IN-6319-OUT;n:type:ShaderForge.SFN_Clamp01,id:6843,x:32044,y:33268,varname:node_6843,prsc:2|IN-4998-OUT;n:type:ShaderForge.SFN_OneMinus,id:2419,x:32226,y:33245,varname:node_2419,prsc:2|IN-6843-OUT;n:type:ShaderForge.SFN_DepthBlend,id:9239,x:31994,y:32660,varname:node_9239,prsc:2|DIST-2281-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2281,x:31810,y:32660,ptovrint:False,ptlb:soft-particle,ptin:_softparticle,varname:node_2281,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_SwitchProperty,id:2434,x:32183,y:32632,ptovrint:False,ptlb:soft-,ptin:_soft,varname:node_2434,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-6309-OUT,B-9239-OUT;n:type:ShaderForge.SFN_Vector1,id:6309,x:31994,y:32557,varname:node_6309,prsc:2,v1:1;proporder:819-2281-2434;pass:END;sub:END;*/

Shader "XIXI/Refractian" {
    Properties {
        _node_819 ("node_819", 2D) = "white" {}
        _softparticle ("soft-particle", Float ) = 0
        [MaterialToggle] _soft ("soft-", Float ) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 100
        GrabPass{ }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _GrabTexture;
            uniform sampler2D _node_819; uniform float4 _node_819_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                float4 vertexColor : COLOR;
                float4 projPos : TEXCOORD5;
                UNITY_FOG_COORDS(6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float4 _node_819_var = tex2D(_node_819,TRANSFORM_TEX(i.uv0, _node_819));
                float3 normalLocal = _node_819_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float2 sceneUVs = (i.projPos.xy / i.projPos.w) + (float2(_node_819_var.r,_node_819_var.g)*i.vertexColor.a*(1.0 - saturate(length((i.uv0*2.0+-1.0)))));
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
////// Lighting:
                float3 finalColor = 0;
                fixed4 finalRGBA = fixed4(lerp(sceneColor.rgb, finalColor,0.0),1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
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
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
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
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
