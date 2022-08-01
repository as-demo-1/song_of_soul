Shader "Unlit/SeedShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        _Boundery ("Boundery", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Boundery;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float blendArea = 0.1;
                float _BounderyWithBlend = _Boundery + blendArea;
                if(i.uv.y > _BounderyWithBlend) discard;
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float factor = (_BounderyWithBlend - i.uv.y) / blendArea;
                factor = clamp(factor, 0, 1);
                col.a = (1.0, 0,  factor);
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
