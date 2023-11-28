Shader "Hidden/Template/ShadowCaster"
{
    Properties
    {
        // _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            colorMask 0
            zwrite on
            
            // Tags{"LightMode"="ShadowCaster"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "../../../../PowerShaderLib/Lib/UnityLib.hlsl"
            #include "../../../../PowerShaderLib/URPLib/URP_MainLightShadows.hlsl"

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
            };


            CBUFFER_START(UnityPerMaterial)
            // float4 _MainTex_ST;
            CBUFFER_END
            float3 _LightDirection;

            v2f vert (appdata v)
            {
                float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
                float3 worldNormal = TransformObjectToWorldNormal(v.normal);

                v2f o = (v2f)0;
                worldPos = ApplyShadowBias(worldPos,worldNormal,_LightDirection,0,0);

                o.vertex = TransformWorldToHClip(worldPos);
                o.uv = v.uv;

                #if UNITY_REVERSED_Z
                    o.vertex.z = min(o.vertex.z, o.vertex.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    o.vertex.z = max(o.vertex.z, o.vertex.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
