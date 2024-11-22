
#if !defined(BRG_INPUT_HLSL)
#define BRG_INPUT_HLSL

#if defined(UNITY_DOTS_INSTANCING_ENABLED)
    // UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
    SRP_BUFFER_START(MaterialPropertyMetadata)
        // UNITY_DOTS_INSTANCED_PROP(float4,_Color)
        // UNITY_DOTS_INSTANCED_PROP(float4,_MainTex_ST)
        DEF_VAR(float4,_Color)
        DEF_VAR(float4,_MainTex_ST)
    // UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
    SRP_BUFFER_END

    #define _Color GET_VAR(float4,_Color)
    #define _MainTex_ST GET_VAR(float4,_MainTex_ST)

#endif
#endif //BRG_INPUT_HLSL