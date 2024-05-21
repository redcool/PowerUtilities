/**
    cmd.BlitTriangle
*/
Shader "Hidden/Utils/DrawScreenSpaceShadow"
{
    Properties
    {
        // _SourceTex ("Texture", 2D) = "white" {}
    }

    HLSLINCLUDE
    #include "../../../../../PowerShaderLib/Lib/UnityLib.hlsl"
    #include "../../../../../PowerShaderLib/Lib/BlitLib.hlsl"
    #include "../../../../../PowerShaderLib/Lib/Colors.hlsl"
    #include "../../../../../PowerShaderLib/Lib/DepthLib.hlsl"
    #define _MAIN_LIGHT_SHADOWS_SCREEN
    #include "../../../../../PowerShaderLib/URPLib/URP_MainLightShadows.hlsl"

    struct v2f
    {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
    };

    sampler2D _SourceTex;
    sampler2D _CameraOpaqueTexture;
    sampler2D _CameraDepthTexture;

    v2f vert (uint vid:SV_VERTEXID)
    {
        v2f o;
        FullScreenTriangleVert(vid,o.vertex/**/,o.uv/**/);

        return o;
    }

    float4 frag (v2f i) : SV_Target
    {
        float2 uv = i.uv;
        float rawDepth = tex2D(_CameraDepthTexture,uv);
        float3 worldPos = ScreenToWorldPos(uv,rawDepth,UNITY_MATRIX_I_VP);
        float4 shadowCoord = TransformWorldToShadowCoord(worldPos);
        float shadow = CalcShadow(shadowCoord,worldPos,1,1);
        float4 col = tex2D(_CameraOpaqueTexture,uv);
        return col * shadow;
    }
    ENDHLSL

    SubShader
    {
        Cull off
        zwrite off
        ztest always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}
