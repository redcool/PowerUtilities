Shader "Hidden/SSPR/HashResolve"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            SamplerState sampler_linear_repeat;
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = (v.uv);
                return o;
            }
    ENDHLSL
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            Texture2D<uint> _MainTex;
            TEXTURE2D(_CameraOpaqueTexture);SAMPLER(sampler_CameraOpaqueTexture);
            float4 _CameraOpaqueTexture_TexelSize;


            half4 Load(float2 screenPos){
                uint hash = LOAD_TEXTURE2D(_MainTex, screenPos);
                half4 c = 0;
                if(hash < 0xffffffff){
                    float2 pos = float2(hash & 0xffff,hash >> 16);
                    c = LOAD_TEXTURE2D(_CameraOpaqueTexture,pos);

                    // float2 uv = pos /_CameraOpaqueTexture_TexelSize.xy;
                    // return SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,uv);

                    // half4 c = 0;
                    // c+= LOAD_TEXTURE2D(_CameraOpaqueTexture,pos+int2(-1,-1));
                    // c+= LOAD_TEXTURE2D(_CameraOpaqueTexture,pos+int2(-1,1));
                    // c+= LOAD_TEXTURE2D(_CameraOpaqueTexture,pos+int2(1,1));
                    // c+= LOAD_TEXTURE2D(_CameraOpaqueTexture,pos+int2(1,-1));
                    // return c*0.25;
                }
                return c;
            }

            half4 frag (v2f i) : SV_Target
            {
                return Load(i.vertex.xy);
            }
            ENDHLSL
        }
        //2
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            // TEXTURE2D(_CameraOpaqueTexture);


            half4 frag (v2f i) : SV_Target
            {
               
                return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
            }
            ENDHLSL
        }
    }
}
