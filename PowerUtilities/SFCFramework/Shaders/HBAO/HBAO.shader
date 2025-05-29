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
            #include "../../../../../PowerShaderLib/Lib/AOLib.hlsl"

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

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                float3 worldPos = ScreenToWorld(uv);
                float3 viewPos = WorldToViewPos(worldPos);
                float4 screenCol = GetScreenColor(uv);

                #if defined(_NORMAL_FROM_DEPTH)
                float3 worldNormal = CalcWorldNormal(worldPos);
                #else
                float3 worldNormal = GetScreenNormal(uv);
                #endif
                float3 viewNormal = normalize(WorldToViewNormal(worldNormal));

                float occlusion = CalcHBAO(uv,viewNormal,viewPos,_DirCount,_StepCount,_StepScale,_AORangeMin,_AORangeMax);
                
                return half4(screenCol.xyz * occlusion,1);
            }
            ENDHLSL
        }
    }
}
