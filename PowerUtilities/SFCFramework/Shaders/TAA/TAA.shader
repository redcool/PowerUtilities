Shader "Hidden/Unlit/TAA"
{
    Properties
    {
        // _SourceTex ("Texture", 2D) = "white" {}
        _TemporalFade("_TemporalFade",range(0,1)) = 0.95
        _MovementBlending("_MovementBlending",float) = 100
        _TexelSizeScale("_TexelSizeScale",range(0.01,1)) = .1
    }

    HLSLINCLUDE
            /*
            Box intersection by IQ, modified for neighbourhood clamping
            https://www.iquilezles.org/www/articles/boxfunctions/boxfunctions.htm
            */
            float2 boxIntersection(in float3 ro, in float3 rd, in float3 rad)
            {
                float3 m = 1.0 / rd;
                float3 n = m * ro;
                float3 k = abs(m) * rad;
                float3 t1 = -n - k;
                float3 t2 = -n + k;

                float tN = max(max(t1.x, t1.y), t1.z);
                float tF = min(min(t2.x, t2.y), t2.z);

                return float2(tN, tF);
            }

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

            sampler2D _SourceTex;
            sampler2D _TemporalAATexture;
            // sampler2D _CameraDepthTexture;

            CBUFFER_START(UnityPerMaterial)
            // float4 _SourceTex_ST;
            float _TexelSizeScale;
            float _TemporalFade;
            float _MovementBlending;
            CBUFFER_END

            float4x4 _invP;
            float4x4 _FrameMatrix;
            float4 _SourceTex_TexelSize;

            // define offsets(3x3,2x2)
            DEF_OFFSETS_3X3(offsets_3x3,_SourceTex_TexelSize.xy);
            DEF_OFFSETS_2X2(offsets_2x2,_SourceTex_TexelSize.xy);
            #define OFFSETS_COUNT 9
            #define OFFSETS offsets_3x3

            v2f vert (uint vid:SV_VERTEXID)
            {
                v2f o;
                FullScreenTriangleVert(vid,o.vertex/**/,o.uv/**/);

                return o;
            }



            float4 frag (v2f i) : SV_Target
            {
                float2 suv = i.vertex.xy/_ScaledScreenParams.xy;

                float depth = (GetScreenDepth(suv));
                float depth01 = LinearDepth01(depth);
                // depth01 = Linear01Depth(depth,_ZBufferParams);
                // depth01 = (depth01*(_ProjectionParams.z - _ProjectionParams.y) + _ProjectionParams.y)/_ProjectionParams.z;
                // return depth01;
                float3 curCol = tex2D(_SourceTex,suv).xyz;
                
                float3 pos = float3(suv*2-1,1);
                float4 viewPos = mul(_invP,pos.xyzz);
                viewPos.xyz /= viewPos.w;

                float4 tuv = mul(_FrameMatrix,float4(viewPos.xyz * depth01,1));
                tuv /= tuv.w;
                float3 lastCol = tex2D(_TemporalAATexture,tuv*0.5+0.5).xyz;

                if(any(abs(tuv)>1))
                    lastCol = curCol;

                float3 ya = curCol.xyz;
                float3 minCol = ya;
                float3 maxCol = ya;

                float3 minColSharpen = 0;
                float3 maxColSharpen = 0;

                // for(int x=-1;x<=1;x++){
                //     for(int y=-1;y<=1;y++){
                //         float2 duv = float2(x,y)/_ScaledScreenParams.xy;
                //         float3 col = tex2D(_SourceTex,suv + duv).xyz;
                //         minCol = min(minCol,col);
                //         maxCol = max(maxCol,col);
                //     }
                // }

                for(int x=0;x<OFFSETS_COUNT;x++){
                    float2 duv = OFFSETS[x] * _TexelSizeScale;
                    float3 col = tex2D(_SourceTex,suv + duv).xyz;
                    minCol = min(minCol,col);
                    maxCol = max(maxCol,col);

                    minColSharpen += minCol * kernels_sharpen[x];
                    maxColSharpen += maxCol * kernels_sharpen[x];
                }

float3 colSharpen = (minColSharpen+maxColSharpen).xyz * 0.5;
// return colSharpen.xyzx;
                lastCol = clamp(lastCol,minCol,maxCol);

                float velocity = length(pos.xy - tuv);
                // return velocity <0.0000001;
                float temporalScale = exp(-_MovementBlending * velocity);
                float3 finalCol = lerp(curCol,lastCol, _TemporalFade * temporalScale);

                return float4(finalCol,1);
            }
    ENDHLSL
    SubShader
    {
        LOD 100
        cull back zwrite off ztest always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}