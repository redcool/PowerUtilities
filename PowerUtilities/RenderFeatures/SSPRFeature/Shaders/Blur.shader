Shader "Hidden/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../../../../../PowerShaderLib/Lib/BlurLib.hlsl"
            SAMPLER(sampler_linear_repeat);

            int _StepCount;
            float _BlurSize;
            
            TEXTURE2D(_MainTex);
            float4 _MainTex_TexelSize;
            Texture2D<uint> _HashResult;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = (v.uv);
                return o;
            }
    ENDHLSL

    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            half4 frag (v2f i) : SV_Target
            {
                uint hash = LOAD_TEXTURE2D(_HashResult, i.vertex.xy);
                float fading = 1;
                if(hash < 0xffffffff){
                    float2 pos = float2(hash & 0xffff,hash >> 16);
                    #if defined(UNITY_UV_STARTS_AT_TOP)
                    fading = pos.y;
                    #else
                    fading = (1-pos.y)*10;
                    #endif
                }
float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_linear_repeat,i.uv);
                float2 uvOffset = _MainTex_TexelSize.xy * _BlurSize;
                float4 blurCol = BoxBlur(_MainTex,sampler_linear_repeat,i.uv,uvOffset * float2(1,0),_StepCount);
                return blurCol;
                // return lerp(col,blurCol,saturate(fading));
            }
            ENDHLSL
        }
        // 1
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            half4 frag (v2f i) : SV_Target
            {
                float2 uvOffset = _MainTex_TexelSize.xy * _BlurSize* float2(0,1);
                return BoxBlur(_MainTex,sampler_linear_repeat,i.uv,uvOffset,_StepCount);
            }
            ENDHLSL
        }        
    }
}
