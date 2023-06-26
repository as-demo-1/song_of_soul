// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:1761,x:33362,y:32657,varname:node_1761,prsc:2|emission-8018-OUT,alpha-1254-OUT,voffset-9944-OUT;n:type:ShaderForge.SFN_Tex2d,id:5485,x:31911,y:32532,ptovrint:False,ptlb:Textures,ptin:_Textures,varname:node_5485,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-6285-UVOUT;n:type:ShaderForge.SFN_VertexColor,id:5062,x:32515,y:32638,varname:node_5062,prsc:2;n:type:ShaderForge.SFN_Color,id:3569,x:32532,y:32430,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_3569,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:8018,x:32766,y:32585,varname:node_8018,prsc:2|A-3569-RGB,B-5485-RGB,C-5062-RGB;n:type:ShaderForge.SFN_SwitchProperty,id:4275,x:32151,y:32697,ptovrint:False,ptlb:TextureAlpha,ptin:_TextureAlpha,varname:node_4275,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-5485-R,B-5485-A;n:type:ShaderForge.SFN_DepthBlend,id:5518,x:32222,y:33114,varname:node_5518,prsc:2;n:type:ShaderForge.SFN_SwitchProperty,id:8806,x:32444,y:33071,ptovrint:False,ptlb:SoftParticle,ptin:_SoftParticle,varname:node_8806,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-8328-OUT,B-5518-OUT;n:type:ShaderForge.SFN_Vector1,id:8328,x:32222,y:33035,varname:node_8328,prsc:2,v1:1;n:type:ShaderForge.SFN_TexCoord,id:1130,x:30745,y:32298,varname:node_1130,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Time,id:3610,x:30532,y:32369,varname:node_3610,prsc:2;n:type:ShaderForge.SFN_ValueProperty,id:8092,x:30555,y:32199,ptovrint:False,ptlb:U Speed,ptin:_USpeed,varname:node_8092,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:899,x:30557,y:32540,ptovrint:False,ptlb:V Speed,ptin:_VSpeed,varname:node_899,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Multiply,id:6103,x:30769,y:32476,varname:node_6103,prsc:2|A-3610-T,B-899-OUT;n:type:ShaderForge.SFN_Multiply,id:2835,x:30757,y:32152,varname:node_2835,prsc:2|A-8092-OUT,B-3610-T;n:type:ShaderForge.SFN_Append,id:7461,x:31127,y:32317,varname:node_7461,prsc:2|A-5074-OUT,B-31-OUT;n:type:ShaderForge.SFN_Add,id:5074,x:30941,y:32229,varname:node_5074,prsc:2|A-2835-OUT,B-1130-U;n:type:ShaderForge.SFN_Add,id:31,x:30941,y:32426,varname:node_31,prsc:2|A-1130-V,B-6103-OUT;n:type:ShaderForge.SFN_Multiply,id:7551,x:32760,y:32925,varname:node_7551,prsc:2|A-7234-OUT,B-5062-A,C-8806-OUT,D-24-OUT,E-6339-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:7234,x:32515,y:32836,ptovrint:False,ptlb:Mask Tex,ptin:_MaskTex,varname:node_7234,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-4275-OUT,B-1096-OUT;n:type:ShaderForge.SFN_Tex2d,id:272,x:31986,y:33097,ptovrint:False,ptlb:Opacity Textures,ptin:_OpacityTextures,varname:node_272,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-5687-OUT;n:type:ShaderForge.SFN_Multiply,id:1096,x:32226,y:32872,varname:node_1096,prsc:2|A-4275-OUT,B-272-R;n:type:ShaderForge.SFN_Fresnel,id:9623,x:32251,y:33328,varname:node_9623,prsc:2|EXP-8418-OUT;n:type:ShaderForge.SFN_OneMinus,id:6208,x:32421,y:33338,varname:node_6208,prsc:2|IN-9623-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8418,x:32086,y:33362,ptovrint:False,ptlb: Fresnel  Power,ptin:_FresnelPower,varname:node_8418,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_SwitchProperty,id:24,x:32615,y:33312,ptovrint:False,ptlb:Fresnel Op,ptin:_FresnelOp,varname:node_24,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-292-OUT,B-6208-OUT;n:type:ShaderForge.SFN_Vector1,id:292,x:32381,y:33248,varname:node_292,prsc:2,v1:1;n:type:ShaderForge.SFN_Tex2d,id:7131,x:32256,y:33960,ptovrint:False,ptlb:Dissolve Tex,ptin:_DissolveTex,varname:node_7131,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_TexCoord,id:9059,x:31773,y:33735,varname:node_9059,prsc:2,uv:1,uaff:True;n:type:ShaderForge.SFN_SwitchProperty,id:6339,x:32819,y:33578,ptovrint:False,ptlb:Dissolve,ptin:_Dissolve,varname:node_6339,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-168-OUT,B-6768-OUT;n:type:ShaderForge.SFN_Vector1,id:168,x:32633,y:33547,varname:node_168,prsc:2,v1:1;n:type:ShaderForge.SFN_Add,id:7235,x:31681,y:32614,varname:node_7235,prsc:2|A-3347-OUT,B-3119-OUT;n:type:ShaderForge.SFN_Tex2d,id:8343,x:30945,y:33044,ptovrint:False,ptlb:UV Dis,ptin:_UVDis,varname:node_8343,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-5066-OUT;n:type:ShaderForge.SFN_Multiply,id:3119,x:31230,y:32959,varname:node_3119,prsc:2|A-317-OUT,B-8343-R;n:type:ShaderForge.SFN_ValueProperty,id:317,x:30945,y:32949,ptovrint:False,ptlb:UV Dis Power,ptin:_UVDisPower,varname:node_317,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_SwitchProperty,id:3347,x:31436,y:32590,ptovrint:False,ptlb:PanUV,ptin:_PanUV,varname:node_9480,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-7461-OUT,B-2108-OUT;n:type:ShaderForge.SFN_Add,id:2108,x:31203,y:32614,varname:node_2108,prsc:2|A-2258-UVOUT,B-6374-OUT;n:type:ShaderForge.SFN_Append,id:6374,x:31006,y:32773,varname:node_6374,prsc:2|A-2942-Z,B-2942-W;n:type:ShaderForge.SFN_TexCoord,id:2942,x:30782,y:32773,varname:node_2942,prsc:2,uv:1,uaff:True;n:type:ShaderForge.SFN_TexCoord,id:2258,x:30782,y:32621,varname:node_2258,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Multiply,id:1254,x:32879,y:32811,varname:node_1254,prsc:2|A-3569-A,B-7551-OUT;n:type:ShaderForge.SFN_TexCoord,id:2584,x:31218,y:33371,varname:node_2584,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Time,id:2381,x:31005,y:33442,varname:node_2381,prsc:2;n:type:ShaderForge.SFN_ValueProperty,id:7129,x:31028,y:33272,ptovrint:False,ptlb:MaskU speed,ptin:_MaskUspeed,varname:_USpeed_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:9397,x:31030,y:33613,ptovrint:False,ptlb:MaskV spped,ptin:_MaskVspped,varname:_VSpeed_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Multiply,id:4624,x:31242,y:33549,varname:node_4624,prsc:2|A-2381-T,B-9397-OUT;n:type:ShaderForge.SFN_Multiply,id:9076,x:31230,y:33225,varname:node_9076,prsc:2|A-7129-OUT,B-2381-T;n:type:ShaderForge.SFN_Append,id:5687,x:31600,y:33390,varname:node_5687,prsc:2|A-9855-OUT,B-9824-OUT;n:type:ShaderForge.SFN_Add,id:9855,x:31414,y:33302,varname:node_9855,prsc:2|A-9076-OUT,B-2584-U;n:type:ShaderForge.SFN_Add,id:9824,x:31414,y:33499,varname:node_9824,prsc:2|A-2584-V,B-4624-OUT;n:type:ShaderForge.SFN_Rotator,id:6285,x:31755,y:32769,varname:node_6285,prsc:2|UVIN-7235-OUT,ANG-9625-OUT;n:type:ShaderForge.SFN_Multiply,id:1006,x:31560,y:33045,varname:node_1006,prsc:2|A-1844-OUT,B-1610-OUT;n:type:ShaderForge.SFN_Pi,id:1610,x:31447,y:33077,varname:node_1610,prsc:2;n:type:ShaderForge.SFN_ValueProperty,id:1844,x:31414,y:32993,ptovrint:False,ptlb:旋转角度,ptin:_,varname:node_1844,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Divide,id:9625,x:31729,y:33045,varname:node_9625,prsc:2|A-1006-OUT,B-4693-OUT;n:type:ShaderForge.SFN_Vector1,id:4693,x:31526,y:33206,varname:node_4693,prsc:2,v1:180;n:type:ShaderForge.SFN_TexCoord,id:9418,x:30350,y:33069,varname:node_9418,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Multiply,id:9308,x:30374,y:33247,varname:node_9308,prsc:2|A-1941-T,B-6714-OUT;n:type:ShaderForge.SFN_Multiply,id:3973,x:30362,y:32923,varname:node_3973,prsc:2|A-1702-OUT,B-1941-T;n:type:ShaderForge.SFN_Append,id:5066,x:30732,y:33088,varname:node_5066,prsc:2|A-5384-OUT,B-3400-OUT;n:type:ShaderForge.SFN_Add,id:5384,x:30546,y:33000,varname:node_5384,prsc:2|A-3973-OUT,B-9418-U;n:type:ShaderForge.SFN_Add,id:3400,x:30546,y:33197,varname:node_3400,prsc:2|A-9418-V,B-9308-OUT;n:type:ShaderForge.SFN_Time,id:1941,x:30137,y:33140,varname:node_1941,prsc:2;n:type:ShaderForge.SFN_ValueProperty,id:1702,x:30143,y:32873,ptovrint:False,ptlb:UVDiss_USpeed,ptin:_UVDiss_USpeed,varname:node_1702,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:6714,x:30099,y:33355,ptovrint:False,ptlb:UVDiss_V_Speed,ptin:_UVDiss_V_Speed,varname:node_6714,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Tex2d,id:3834,x:32988,y:33026,ptovrint:False,ptlb:VertexOffset,ptin:_VertexOffset,varname:node_3834,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:9944,x:33181,y:33100,varname:node_9944,prsc:2|A-3834-RGB,B-9921-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9921,x:32988,y:33208,ptovrint:False,ptlb:VertexPower,ptin:_VertexPower,varname:node_9921,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_OneMinus,id:4159,x:31988,y:33577,varname:node_4159,prsc:2|IN-9059-U;n:type:ShaderForge.SFN_SwitchProperty,id:4361,x:32175,y:33577,ptovrint:False,ptlb:disslove or Power,ptin:_dissloveorPower,varname:node_4361,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-4159-OUT,B-1939-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1939,x:31988,y:33796,ptovrint:False,ptlb:Disslove Power,ptin:_DisslovePower,varname:node_1939,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Add,id:7675,x:32365,y:33668,varname:node_7675,prsc:2|A-4361-OUT,B-7637-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7637,x:32175,y:33796,ptovrint:False,ptlb:Smooth,ptin:_Smooth,varname:node_7637,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Smoothstep,id:6768,x:32633,y:33629,varname:node_6768,prsc:2|A-4361-OUT,B-7675-OUT,V-7131-R;proporder:5485-3569-4275-8806-8092-899-7234-272-8418-24-7131-6339-8343-317-3347-7129-9397-1844-1702-6714-3834-9921-4361-1939-7637;pass:END;sub:END;*/

Shader "XIXI/Alpha" {
    Properties {
        _Textures ("Textures", 2D) = "white" {}
        [HDR]_Color ("Color", Color) = (0.5,0.5,0.5,1)
        [MaterialToggle] _TextureAlpha ("TextureAlpha", Float ) = 0
        [MaterialToggle] _SoftParticle ("SoftParticle", Float ) = 1
        _USpeed ("U Speed", Float ) = 0
        _VSpeed ("V Speed", Float ) = 0
        [MaterialToggle] _MaskTex ("Mask Tex", Float ) = 0
        _OpacityTextures ("Opacity Textures", 2D) = "white" {}
        _FresnelPower (" Fresnel  Power", Float ) = 0
        [MaterialToggle] _FresnelOp ("Fresnel Op", Float ) = 1
        _DissolveTex ("Dissolve Tex", 2D) = "white" {}
        [MaterialToggle] _Dissolve ("Dissolve", Float ) = 1
        _UVDis ("UV Dis", 2D) = "white" {}
        _UVDisPower ("UV Dis Power", Float ) = 0
        [MaterialToggle] _PanUV ("PanUV", Float ) = 0
        _MaskUspeed ("MaskU speed", Float ) = 0
        _MaskVspped ("MaskV spped", Float ) = 0
        _ ("旋转角度", Float ) = 0
        _UVDiss_USpeed ("UVDiss_USpeed", Float ) = 0
        _UVDiss_V_Speed ("UVDiss_V_Speed", Float ) = 0
        _VertexOffset ("VertexOffset", 2D) = "white" {}
        _VertexPower ("VertexPower", Float ) = 0
        [MaterialToggle] _dissloveorPower ("disslove or Power", Float ) = 1
        _DisslovePower ("Disslove Power", Float ) = 0
        _Smooth ("Smooth", Float ) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 100
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _CameraDepthTexture;
            uniform sampler2D _Textures; uniform float4 _Textures_ST;
            uniform float4 _Color;
            uniform fixed _TextureAlpha;
            uniform fixed _SoftParticle;
            uniform float _USpeed;
            uniform float _VSpeed;
            uniform fixed _MaskTex;
            uniform sampler2D _OpacityTextures; uniform float4 _OpacityTextures_ST;
            uniform float _FresnelPower;
            uniform fixed _FresnelOp;
            uniform sampler2D _DissolveTex; uniform float4 _DissolveTex_ST;
            uniform fixed _Dissolve;
            uniform sampler2D _UVDis; uniform float4 _UVDis_ST;
            uniform float _UVDisPower;
            uniform fixed _PanUV;
            uniform float _MaskUspeed;
            uniform float _MaskVspped;
            uniform float _;
            uniform float _UVDiss_USpeed;
            uniform float _UVDiss_V_Speed;
            uniform sampler2D _VertexOffset; uniform float4 _VertexOffset_ST;
            uniform float _VertexPower;
            uniform fixed _dissloveorPower;
            uniform float _DisslovePower;
            uniform float _Smooth;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                float4 vertexColor : COLOR;
                float4 projPos : TEXCOORD4;
                UNITY_FOG_COORDS(5)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 _VertexOffset_var = tex2Dlod(_VertexOffset,float4(TRANSFORM_TEX(o.uv0, _VertexOffset),0.0,0));
                v.vertex.xyz += (_VertexOffset_var.rgb*_VertexPower);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float sceneZ = max(0,LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)))) - _ProjectionParams.g);
                float partZ = max(0,i.projPos.z - _ProjectionParams.g);
////// Lighting:
////// Emissive:
                float node_6285_ang = ((_*3.141592654)/180.0);
                float node_6285_spd = 1.0;
                float node_6285_cos = cos(node_6285_spd*node_6285_ang);
                float node_6285_sin = sin(node_6285_spd*node_6285_ang);
                float2 node_6285_piv = float2(0.5,0.5);
                float4 node_3610 = _Time;
                float4 node_1941 = _Time;
                float2 node_5066 = float2(((_UVDiss_USpeed*node_1941.g)+i.uv0.r),(i.uv0.g+(node_1941.g*_UVDiss_V_Speed)));
                float4 _UVDis_var = tex2D(_UVDis,TRANSFORM_TEX(node_5066, _UVDis));
                float2 node_6285 = (mul((lerp( float2(((_USpeed*node_3610.g)+i.uv0.r),(i.uv0.g+(node_3610.g*_VSpeed))), (i.uv0+float2(i.uv1.b,i.uv1.a)), _PanUV )+(_UVDisPower*_UVDis_var.r))-node_6285_piv,float2x2( node_6285_cos, -node_6285_sin, node_6285_sin, node_6285_cos))+node_6285_piv);
                float4 _Textures_var = tex2D(_Textures,TRANSFORM_TEX(node_6285, _Textures));
                float3 emissive = (_Color.rgb*_Textures_var.rgb*i.vertexColor.rgb);
                float3 finalColor = emissive;
                float _TextureAlpha_var = lerp( _Textures_var.r, _Textures_var.a, _TextureAlpha );
                float4 node_2381 = _Time;
                float2 node_5687 = float2(((_MaskUspeed*node_2381.g)+i.uv0.r),(i.uv0.g+(node_2381.g*_MaskVspped)));
                float4 _OpacityTextures_var = tex2D(_OpacityTextures,TRANSFORM_TEX(node_5687, _OpacityTextures));
                float _dissloveorPower_var = lerp( (1.0 - i.uv1.r), _DisslovePower, _dissloveorPower );
                float4 _DissolveTex_var = tex2D(_DissolveTex,TRANSFORM_TEX(i.uv0, _DissolveTex));
                fixed4 finalRGBA = fixed4(finalColor,(_Color.a*(lerp( _TextureAlpha_var, (_TextureAlpha_var*_OpacityTextures_var.r), _MaskTex )*i.vertexColor.a*lerp( 1.0, saturate((sceneZ-partZ)), _SoftParticle )*lerp( 1.0, (1.0 - pow(1.0-max(0,dot(normalDirection, viewDirection)),_FresnelPower)), _FresnelOp )*lerp( 1.0, smoothstep( _dissloveorPower_var, (_dissloveorPower_var+_Smooth), _DissolveTex_var.r ), _Dissolve ))));
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
            Cull Back
            
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
            uniform sampler2D _VertexOffset; uniform float4 _VertexOffset_ST;
            uniform float _VertexPower;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                float4 _VertexOffset_var = tex2Dlod(_VertexOffset,float4(TRANSFORM_TEX(o.uv0, _VertexOffset),0.0,0));
                v.vertex.xyz += (_VertexOffset_var.rgb*_VertexPower);
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
