Shader "Hidden/SSPR/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../../../../../PowerShaderLib/Lib/BlurLib.hlsl"
            #include "../../../../../PowerShaderLib/Lib/ScreenTextures.hlsl"
            SAMPLER(sampler_linear_repeat);

            int _StepCount;
            float _BlurSize;
            Texture2D<uint> _HashResult;
            
            TEXTURE2D(_MainTex);

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_TexelSize;
            CBUFFER_END

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
            #pragma multi_compile _ _SSPR_BLUR_SINGLE_PASS

            half4 frag (v2f i) : SV_Target
            {
                float2 screenUV= i.vertex.xy / _ScaledScreenParams.xy;
                float depthTex = GetScreenDepth(screenUV);
                float fading = 1;
                
                uint hash = LOAD_TEXTURE2D(_HashResult, i.vertex.xy);
                float2 refUV = float2(hash & 0xffff,hash >> 16);
                fading = refUV.y/_ScaledScreenParams.y;

                bool isReflected = hash < 0xffffffff;
                
                float2 pos = float2(hash & 0xffff,hash >> 16);
                #if defined(UNITY_UV_STARTS_AT_TOP)
                fading = pos.y;
                #else
                fading = (1-pos.y)*1;
                #endif
                fading *= isReflected;
                
                half4 col = 0;
                float2 uv = i.uv;
                uv += 0.5 * _MainTex_TexelSize.xy * _BlurSize;
                col = SAMPLE_TEXTURE2D(_MainTex,sampler_linear_repeat,uv);

                float2 uvOffset = _MainTex_TexelSize.xy * _BlurSize;
                float4 blurCol = BoxBlur(_MainTex,sampler_linear_repeat,uv,uvOffset * float2(1,0),_StepCount);
                #if defined(_SSPR_BLUR_SINGLE_PASS)
                    blurCol += BoxBlur(_MainTex,sampler_linear_repeat,uv,uvOffset * float2(0,1),_StepCount);
                    blurCol *= 0.5;
                #endif
                // return blurCol;
                return lerp(col,blurCol,saturate(fading));
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
