/**
    hbao ref:
    https://github.com/karonator/cg-horizon-based-ambient-occlusion.git
*/
Shader "Hidden/Unlit/HBAO"
{
    Properties
    {
        _AORangeMin("_AORangeMin",range(0,1)) = 0.1
        _AORangeMax("_AORangeMax",range(0,1)) = 1
        _StepScale("_StepScale",range(0.02,.2)) = 0.1

        _DirCount("_DirCount",float) = 10
        _StepCount("_StepCount",float) = 4
        
        [GroupToggle(,_NORMAL_FROM_DEPTH)] _NormalFromDepth("_NormalFromDepth",float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        cull off zwrite off ztest always

        Pass
        {
            name "hbao"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _NORMAL_FROM_DEPTH

            #include "../../../../../PowerShaderLib/Lib/UnityLib.hlsl"
            #include "../../../../../PowerShaderLib/Lib/BlitLib.hlsl"
            #include "../../../../../PowerShaderLib/Lib/ScreenTextures.hlsl"

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

            // sampler2D _CameraOpaqueTexture;
            // sampler2D _CameraDepthTexture;
            // sampler2D _CameraNormalsTexture;

            CBUFFER_START(UnityPerMaterial)
            float _AORangeMax,_AORangeMin;
            float _StepScale;
            float _DirCount,_StepCount;
            CBUFFER_END

            v2f vert (appdata i)
            {
                v2f o = (v2f)0;
                FullScreenTriangleVert(i.vertexId,o.vertex/**/,o.uv/**/);
    
                return o;
            }

            float3 WorldToViewPos(float3 worldPos){
                return mul(UNITY_MATRIX_V,float4(worldPos,1)).xyz;
            }
            
            float3 WorldToViewNormal(float3 vec){
                return mul(UNITY_MATRIX_V,float4(vec,0)).xyz;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                float3 worldPos = ScreenToWorld(uv);
                float3 viewPos = WorldToViewPos(worldPos);
                float4 screenCol = GetScreenColor(uv);

                #if defined(_NORMAL_FROM_DEPTH)
                float3 worldNormal = CalcWorldNormal(worldPos);
                #else
                float3 worldNormal = (GetScreenNormal(uv));
                #endif
                float3 viewNormal = normalize(WorldToViewNormal(worldNormal));

                float radiusSS = 64.0 / 512.0;
                int directionsCount =_DirCount;
                int stepsCount = _StepCount;

                float theta = 2 * PI /float(directionsCount);
                float2x2 deltaRotationMatrix = float2x2(
                    cos(theta),-sin(theta),
                    sin(theta),cos(theta)
                );
                float2 deltaUV = float2(radiusSS/(stepsCount+1),0)* _StepScale;
                float occlusion = 0;

                for(int x=0;x<directionsCount ; x++){
                    float horizonAngle = 0.04;
                    deltaUV = mul(deltaRotationMatrix,deltaUV);

                    for(int j=1;j<=stepsCount;j++){
                        float2 sampleUV = uv + j * deltaUV;
                        float3 sampleViewPos = WorldToViewPos(ScreenToWorld(sampleUV));
                        float3 sampleDirVS = sampleViewPos - viewPos;

                        float angle = (PI*0.5) - acos(dot(viewNormal,normalize(sampleDirVS)));
                        if(angle > horizonAngle){
                            float value = sin(angle) - sin(horizonAngle);
                            float attenuation = saturate(1 - pow(length(sampleDirVS)*0.5 , 2));
                            occlusion += value * attenuation;
                            horizonAngle = angle;
                        }
                    }
                }

                occlusion = 1 - occlusion/directionsCount;
                occlusion = smoothstep(_AORangeMin,_AORangeMax,occlusion);
                
                return half4(screenCol.xyz * occlusion,1);
            }
            ENDHLSL
        }
    }
}
