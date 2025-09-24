Shader "Hidden/SSPR/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [GroupToggle()]_OffsetHalfPixelOn("_OffsetHalfPixelOn",float) = 1
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
            half _OffsetHalfPixelOn;
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
            #pragma multi_compile _ _SSPR_OFFSET_HALF_PIXEL _SSPR_BLUR_SINGLE_PASS

            half4 frag (v2f i) : SV_Target
            {
                half4 col = 0;
                float2 uv = i.uv;
                uv += (0.5 * _OffsetHalfPixelOn) * _MainTex_TexelSize.xy;
                col = SAMPLE_TEXTURE2D(_MainTex,sampler_linear_repeat,uv);
                
                col.xyz *= col.w; // w is distance Fading
// return col.w;
                #if defined(_SSPR_OFFSET_HALF_PIXEL)
                    return col;
                #else
                    float2 uvOffset = _MainTex_TexelSize.xy * _BlurSize;
                    float4 blurCol = BoxBlur(_MainTex,sampler_linear_repeat,uv,uvOffset * float2(1,0),_StepCount);
                    #if defined(_SSPR_BLUR_SINGLE_PASS)
                        blurCol += BoxBlur(_MainTex,sampler_linear_repeat,uv,uvOffset * float2(0,1),_StepCount);
                        blurCol *= 0.5;
                    #endif
                    return lerp(col,blurCol,col.w);
                #endif

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
