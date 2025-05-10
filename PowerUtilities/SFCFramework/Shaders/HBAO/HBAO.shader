/**
    hbao ref:
    https://github.com/karonator/cg-horizon-based-ambient-occlusion.git
*/
Shader "Hidden/Unlit/HBAO"
{
    Properties
    {
        [GroupVectorSlider(,AORangeMin AORangeMax,0_1 0_1)]_AORange("_AORange",vector) = (0,1,0,0)
        _StepScale("_StepScale",range(0.02,.2)) = 0.1
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
            float2 _AORange;
            float _StepScale;
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
                float3 worldNormal = (GetScreenNormal(uv));
                float3 viewNormal = normalize(WorldToViewNormal(worldNormal));

                const float radiusSS = 64.0 / 512.0;
                const int directionsCount = 4;
                const int stepsCount = 10;

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
                occlusion = smoothstep(_AORange.x,_AORange.y,occlusion);
                
                // occlusion = saturate(pow(occlusion,2.7));
                // occlusion = pow(occlusion,0.4545);
                // return saturate(occlusion);

                return half4(screenCol.xyz * occlusion,1);
            }
            ENDHLSL
        }
    }
}
