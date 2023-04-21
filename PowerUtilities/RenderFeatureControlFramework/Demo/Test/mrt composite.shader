Shader "Unlit/mrt 1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="OutputMRT" }
        LOD 100

        Pass
        {
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
            struct FragOut{
                float4 col0 : SV_TARGET;
                float4 col1 : SV_TARGET1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _ColorBuffer1;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            float4 _ScaledScreenParams;
            void frag (v2f i, out float4 color:SV_TARGET)
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 suv = i.vertex.xy/_ScaledScreenParams.xy;
                #if UNITY_REVERSED_Z
                    suv.y = 1-suv.y;
                #endif
                half4 col1 = tex2D(_ColorBuffer1,suv);
                color = lerp(col , col1,0.5);
            }
            ENDCG
        }
    }
}
