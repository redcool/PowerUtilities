Shader "Hidden/Template/ReadShadow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            // Tags{"LightMode"="ShadowCaster"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "../../../../PowerShaderLib/Lib/UnityLib.hlsl"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 bigShadowCoord:TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4x4 _BigShadowVP;
            sampler2D _BigShadowMap;

            v2f vert (appdata v)
            {
                v2f o = (v2f)0;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.bigShadowCoord = mul(_BigShadowVP,float4(worldPos,1));
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 tex = tex2D(_MainTex,i.uv);
                float shadow = tex2D(_BigShadowMap,i.bigShadowCoord.xy).x;
                return shadow;
                return tex;
            }
            ENDHLSL
        }
    }
}
