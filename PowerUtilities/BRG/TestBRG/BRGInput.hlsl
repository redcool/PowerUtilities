
#if !defined(BRG_INPUT_HLSL)
#define BRG_INPUT_HLSL

#include "../../../../PowerShaderLib/Lib/InstancingLib.hlsl"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
    float4 color:COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
sampler2D _MainTex;

CBUFFER_START(UnityPerMaterial)
    float4 _MainTex_ST;
    float4 _Color;
CBUFFER_END


#if defined(UNITY_DOTS_INSTANCING_ENABLED)
    // UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
    DOTS_CBUFFER_START(MaterialPropertyMetadata)
        // UNITY_DOTS_INSTANCED_PROP(float4,_Color)
        // UNITY_DOTS_INSTANCED_PROP(float4,_MainTex_ST)
        DEF_VAR(float4,_Color)
        DEF_VAR(float4,_MainTex_ST)
    // UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
    DOTS_CBUFFER_END

    #define _Color GET_VAR(float4,_Color)
    #define _MainTex_ST GET_VAR(float4,_MainTex_ST)

#endif
#endif //BRG_INPUT_HLSL