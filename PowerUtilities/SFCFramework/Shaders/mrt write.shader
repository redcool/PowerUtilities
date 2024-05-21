Shader "Unlit/mrt"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal:NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal:TEXCOORD1;
            };
            struct FragOut{
                float4 col0 : SV_TARGET;
                float4 col1 : SV_TARGET1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            FragOut frag (v2f i)
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                FragOut fragOut = (FragOut)0;
                fragOut.col0 = col;
                fragOut.col1 = float4(i.normal.xyz,0);
                return fragOut;
            }
            ENDCG
        }
    }
}
