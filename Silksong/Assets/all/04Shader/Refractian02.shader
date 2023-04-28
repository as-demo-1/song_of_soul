// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:2768,x:32719,y:32712,varname:node_2768,prsc:2|normal-819-RGB,alpha-6348-OUT,refract-95-OUT;n:type:ShaderForge.SFN_Tex2d,id:819,x:31921,y:32821,ptovrint:False,ptlb:node_819,ptin:_node_819,varname:node_819,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-7153-OUT;n:type:ShaderForge.SFN_VertexColor,id:3116,x:32226,y:33088,varname:node_3116,prsc:2;n:type:ShaderForge.SFN_Multiply,id:95,x:32487,y:33051,varname:node_95,prsc:2|A-4512-OUT,B-3116-A,C-2419-OUT,D-4497-OUT;n:type:ShaderForge.SFN_Append,id:4512,x:32221,y:32941,varname:node_4512,prsc:2|A-819-R,B-819-G;n:type:ShaderForge.SFN_Vector1,id:1480,x:32357,y:32912,varname:node_1480,prsc:2,v1:0;n:type:ShaderForge.SFN_TexCoord,id:4176,x:31516,y:33224,varname:node_4176,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_RemapRange,id:6319,x:31676,y:33240,varname:node_6319,prsc:2,frmn:0,frmx:1,tomn:-1,tomx:1|IN-4176-UVOUT;n:type:ShaderForge.SFN_Length,id:4998,x:31877,y:33268,varname:node_4998,prsc:2|IN-6319-OUT;n:type:ShaderForge.SFN_Clamp01,id:6843,x:32044,y:33268,varname:node_6843,prsc:2|IN-4998-OUT;n:type:ShaderForge.SFN_OneMinus,id:2419,x:32226,y:33245,varname:node_2419,prsc:2|IN-6843-OUT;n:type:ShaderForge.SFN_Color,id:7422,x:31898,y:32594,ptovrint:False,ptlb:node_7422,ptin:_node_7422,varname:node_7422,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:6348,x:32205,y:32713,varname:node_6348,prsc:2|A-7422-A,B-819-A;n:type:ShaderForge.SFN_Multiply,id:6820,x:32169,y:32509,varname:node_6820,prsc:2|A-7422-RGB,B-819-RGB;n:type:ShaderForge.SFN_ValueProperty,id:4497,x:32226,y:33403,ptovrint:False,ptlb:power,ptin:_power,varname:node_4497,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_TexCoord,id:2367,x:31382,y:32786,varname:node_2367,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_ValueProperty,id:1377,x:31147,y:32662,ptovrint:False,ptlb:U_speed,ptin:_U_speed,varname:node_1377,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:578,x:31147,y:32908,ptovrint:False,ptlb:V_speed,ptin:_V_speed,varname:node_578,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Time,id:9345,x:31147,y:32743,varname:node_9345,prsc:2;n:type:ShaderForge.SFN_Multiply,id:7326,x:31382,y:32647,varname:node_7326,prsc:2|A-1377-OUT,B-9345-T;n:type:ShaderForge.SFN_Multiply,id:1823,x:31382,y:32939,varname:node_1823,prsc:2|A-9345-T,B-578-OUT;n:type:ShaderForge.SFN_Add,id:2348,x:31568,y:32647,varname:node_2348,prsc:2|A-7326-OUT,B-2367-U;n:type:ShaderForge.SFN_Add,id:4533,x:31567,y:32920,varname:node_4533,prsc:2|A-2367-V,B-1823-OUT;n:type:ShaderForge.SFN_Append,id:7153,x:31740,y:32776,varname:node_7153,prsc:2|A-2348-OUT,B-4533-OUT;proporder:819-7422-4497-1377-578;pass:END;sub:END;*/

Shader "XIXI/Refractian02" {
    Properties {
        _node_819 ("node_819", 2D) = "white" {}
        [HDR]_node_7422 ("node_7422", Color) = (0.5,0.5,0.5,1)
        _power ("power", Float ) = 0
        _U_speed ("U_speed", Float ) = 0
        _V_speed ("V_speed", Float ) = 0
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
            uniform float4 _node_7422;
            uniform float _power;
            uniform float _U_speed;
            uniform float _V_speed;
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
                float4 node_9345 = _Time;
                float2 node_7153 = float2(((_U_speed*node_9345.g)+i.uv0.r),(i.uv0.g+(node_9345.g*_V_speed)));
                float4 _node_819_var = tex2D(_node_819,TRANSFORM_TEX(node_7153, _node_819));
                float3 normalLocal = _node_819_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float2 sceneUVs = (i.projPos.xy / i.projPos.w) + (float2(_node_819_var.r,_node_819_var.g)*i.vertexColor.a*(1.0 - saturate(length((i.uv0*2.0+-1.0))))*_power);
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
////// Lighting:
                float3 finalColor = 0;
                fixed4 finalRGBA = fixed4(lerp(sceneColor.rgb, finalColor,(_node_7422.a*_node_819_var.a)),1);
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
