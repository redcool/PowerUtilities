Shader "Hidden/kMotion/CameraMotionVectors"
{
    Properties
    {
    [GroupPresetBlendMode(,,_SrcMode,_DstMode)]_PresetBlendMode("_PresetBlendMode",int)=0
    // [GroupEnum(Alpha,UnityEngine.Rendering.BlendMode)]
    [HideInInspector]_SrcMode("_SrcMode",int) = 1
    [HideInInspector]_DstMode("_DstMode",int) = 0
    }
    SubShader
    {
        Pass
        {
            blend [_SrcMode][_DstMode]
            // blendOp [_BlendOp]
            Cull Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            // #pragma exclude_renderers d3d11_9x gles
            #pragma target 3.5

            #pragma vertex vert
            #pragma fragment frag

            // -------------------------------------
            // Includes
            #include "../../../../../PowerShaderLib/Lib/UnityLib.hlsl"
            #include "../../../../../PowerShaderLib/Lib/BlitLib.hlsl"
            #include "../../../../../PowerShaderLib/Lib/ScreenTextures.hlsl"

        #if defined(USING_STEREO_MATRICES)
            float4x4 _PrevViewProjMStereo[2];
            #define _PrevViewProjM _PrevViewProjMStereo[unity_StereoEyeIndex]
        #else
            #define  _PrevViewProjM _PrevViewProjMatrix
        #endif
            float4x4 _PrevIVP;
            // -------------------------------------
            // Structs
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint vertexId:SV_VERTEXID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // -------------------------------------
            // Vertex
            v2f vert (appdata i)
            {
                v2f o = (v2f)0;
                FullScreenTriangleVert(i.vertexId,o.vertex/**/,o.uv/**/);
    
                return o;
            }
            
            TEXTURE2D_FLOAT(_CameraDepthAttachment);
            TEXTURE2D_FLOAT(_CameraDepthTexture2);
            
            // SAMPLER(sampler_CameraDepthAttachment);
            SAMPLER(sampler_point_clamp);

            float2 GetMV(float2 suv){
                float rawDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_point_clamp,suv);
                float3 worldPos = ScreenToWorldPos(suv,rawDepth,UNITY_MATRIX_I_VP); //
                // Calculate positions
                float4 previousPositionVP = mul(_PrevViewProjM, float4(worldPos, 1.0));
                float4 positionVP = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));

                previousPositionVP.xy *= rcp(previousPositionVP.w);
                positionVP.xy *= rcp(positionVP.w);
 
                // Calculate velocity
                float2 velocity = (positionVP.xy - previousPositionVP.xy);
                #if UNITY_UV_STARTS_AT_TOP
                    velocity.y = -velocity.y;
                #endif
                // Convert velocity from Clip space (-1..1) to NDC 0..1 space
                // Note it doesn't mean we don't have negative value, we store negative or positive offset in NDC space.
                // Note: ((positionVP * 0.5 + 0.5) - (previousPositionVP * 0.5 + 0.5)) = (velocity * 0.5)
                return velocity.xy * 0.5;
            }
            /** render pass order:
                1 render scene pass
                2 MotionVector pass
                3 copy depth pass
            */
            float2 GetMV2(float2 suv){
                float rawDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_point_clamp,suv);
                float3 worldPos = ScreenToWorldPos(suv,rawDepth,_PrevIVP); //

                float4 prevPosNDC = mul(_PrevViewProjM,float4(worldPos,0));
                prevPosNDC /= prevPosNDC.w;
                half isFar = IsTooFar(rawDepth.x);

                float curDepth = SAMPLE_TEXTURE2D(_CameraDepthAttachment,sampler_point_clamp,suv);
                float3 curWorldPos = ScreenToWorldPos(suv,curDepth,UNITY_MATRIX_I_VP);
                float4 curPosNDC = mul(UNITY_MATRIX_VP,float4(curWorldPos,0));
                curPosNDC /= curPosNDC.w;

                float2 mv = curPosNDC.xy - prevPosNDC.xy;
                return mv;
            }
            // -------------------------------------
            // Fragment
            half4 frag(v2f i
            // , out float outDepth : SV_Depth
            ) : SV_Target
            {
                float2 suv = i.uv;
                return half4(GetMV(suv),0,0);
            }

            ENDHLSL
        }
    }
}
