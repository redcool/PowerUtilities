Shader "Hidden/Template/TestBigShadow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [GroupToggle(,_SHADOWS_SOFT)]_ShadowSoftOn("_ShadowSoftOn",int) = 0
        _SoftScale("_SoftScale",range(0,10)) = 0
    }

    HLSLINCLUDE
        #include "../../../../PowerShaderLib/Lib/BigShadows.hlsl"

    ENDHLSL
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            // Tags{"LightMode"="ShadowCaster"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _SHADOWS_SOFT
            

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos:TEXCOORD1;
            };

            sampler2D _MainTex;

            float4 _MainTex_ST;
            float _SoftScale;


            v2f vert (appdata v)
            {
                v2f o = (v2f)0;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.worldPos = worldPos;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 bigShadowCoord = TransformWorldToBigShadow(i.worldPos);
                float shadow = CalcBigShadowAtten(bigShadowCoord,_SoftScale);
                return shadow;

                float4 tex = tex2D(_MainTex,i.uv);
                return tex;
            }
            ENDHLSL
        }
    }
}
