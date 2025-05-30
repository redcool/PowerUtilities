Shader "Hidden/Unlit/TAA"
{
    Properties
    {
        // _SourceTex ("Texture", 2D) = "white" {}

        _TemporalFade("_TemporalFade",range(0,1)) = 0.95
        _MovementBlending("_MovementBlending",range(0,100)) = 100
        _TexelSizeScale("_TexelSizeScale",range(1,2)) = 1
        _Sharpen("_Sharpen",range(0.01,1)) = 0.1
        // _DepthOffset("_DepthOffset",range(1,1000)) = 1
    }

    HLSLINCLUDE

            /*
            * GLSL Color Spaces by tobspr
            * https://github.com/tobspr/GLSL-Color-Spaces
            */
            // RGB to YCbCr, ranges [0, 1]
            float3 rgb_to_ycbcr(float3 rgb) {
                float y = 0.299 * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b;
                float cb = (rgb.b - y) * 0.565;
                float cr = (rgb.r - y) * 0.713;

                return float3(y, cb, cr);
            }

            // YCbCr to RGB
            float3 ycbcr_to_rgb(float3 yuv) {
                return float3(
                    yuv.x + 1.403 * yuv.z,
                    yuv.x - 0.344 * yuv.y - 0.714 * yuv.z,
                    yuv.x + 1.770 * yuv.y
                );
            }

            float3 clipLuminance(float3 col, float3 minCol, float3 maxCol){
                float3 c0 = rgb_to_ycbcr(col);
                //float3 c1 = rgb_to_ycbcr(minCol);
                //float3 c2 = rgb_to_ycbcr(maxCol);

                //c0 = clamp(c0, c1, c2);
                c0 = clamp(c0, minCol, maxCol);

                return ycbcr_to_rgb(c0);
            }

            #include "../../../../../PowerShaderLib/Lib/UnityLib.hlsl"
            #include "../../../../../PowerShaderLib/Lib/BlitLib.hlsl"
            #include "../../../../../PowerShaderLib/Lib/ScreenTextures.hlsl"
            #include "../../../../../PowerShaderLib/Lib/Kernel/KernelDefines.hlsl"
            #include "../../../../../PowerShaderLib/Lib/Common/Randoms.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint vertexId:SV_VERTEXID;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };  

            sampler2D _SourceTex;
            float4 _SourceTex_TexelSize;

            sampler2D _TemporalAATexture;
            float4 _TemporalAATexture_TexelSize;

            CBUFFER_START(UnityPerMaterial)
            half _TexelSizeScale;
            half _TemporalFade;
            half _MovementBlending;
            half _Sharpen;
            // half _DepthOffset;
            CBUFFER_END

            float4x4 _invP;
            float4x4 _FrameMatrix;

            // define offsets(3x3,2x2)
            DEF_OFFSETS_3X3(offsets_3x3,_SourceTex_TexelSize.xy);
            DEF_OFFSETS_2X2(offsets_2x2,_SourceTex_TexelSize.xy);

// #define _SAMPLES_3X3
            // define kernels
            #if defined(_SAMPLES_3X3)
                #define OFFSETS_COUNT 9
                #define OFFSETS offsets_3x3
                DEF_KERNELS_3X3(kernels_sharpen_dark,-1,-1,-1,-1,8,-1,-1,-1,-1);
            #else
                #define OFFSETS_COUNT 5
                #define OFFSETS offsets_2x2
                DEF_KERNELS_2X2(kernels_sharpen_dark,-1,-1,4,-1,-1);
            #endif
            

            v2f vert (appdata i)
            {
                v2f o = (v2f)0;
                FullScreenTriangleVert(i.vertexId,o.vertex/**/,o.uv/**/);
    
                return o;
            }
            // override _ScaledScreenParams
            #define _ScaledScreenParams _TemporalAATexture_TexelSize.zwxy

            float4 frag (v2f i) : SV_Target
            {
                float2 suv = i.vertex.xy/_ScaledScreenParams.xy;

                float depth = GetScreenDepth(suv);
                // float depth = tex2D(_CameraDepthAttachment,suv);
                float depth01 = Linear01Depth(depth);
                // return depth01;

                float3 curCol = tex2D(_SourceTex,suv).xyz;

                // stable pos
                float3 pos = float3(suv*2-1,1);
                float4 viewPos = mul(_invP,pos.xyzz);
                viewPos.xyz /= viewPos.w;

                //_FrameMatrix is last Projection,
                // jitter pos, reproject to last ndc
                float4 tuv = mul(_FrameMatrix,float4(viewPos.xyz * depth01,1));
                tuv /= tuv.w;
                float3 lastCol = tex2D(_TemporalAATexture,tuv.xy*0.5+0.5).xyz;

                // if(any(abs(tuv.xy)>1))
                //     lastCol = curCol;

                float3 ya = curCol.xyz;
                float3 minCol = ya;
                float3 maxCol = ya;

                float3 colSharpen = 0;

                UNITY_UNROLL
                for(int x=0;x<OFFSETS_COUNT;x++){
                    float2 duv = OFFSETS[x] * _TexelSizeScale;
                    float3 col = tex2D(_SourceTex,suv + duv).xyz;
                    minCol = min(minCol,col);
                    maxCol = max(maxCol,col);

                    colSharpen += col * kernels_sharpen_dark[x];
                }

                lastCol = clamp(lastCol,minCol,maxCol) ;
                // lastCol *= rcp(lastCol+1.0); // more gray

                // ndc space velocity(can use motionVectors)
                float velocity = length(pos.xy - tuv.xy);
                // return velocity <0.0000001;

                // blend last curent colors
                float temporalScale = exp(-_MovementBlending * velocity);
                float3 finalCol = lerp(curCol,lastCol, _TemporalFade * temporalScale);

                // apply sharpen compensation
                finalCol += colSharpen * _Sharpen*depth01;

                return float4(finalCol,1);
            }
    ENDHLSL
    SubShader
    {
        LOD 100
        cull off zwrite off ztest always

        Pass
        {
            HLSLPROGRAM
            #pragma multi_compile _ _SAMPLES_3X3
            #pragma vertex vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}
