Shader "Unlit/ShowReflectionTexture"
{
    Properties
    {
        _Smoothness("_Smoothness",range(0,1)) = 0
        _IBL("ibl",cube) = ""{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "queue"="geometry+100"}
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal:NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal:TEXCOORD1;
                float3 worldPos:TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _ReflectionTexture;

            float4 _ScaledScreenParams;
            float _Smoothness;
            
            samplerCUBE _IBL;
            float4 _IBL_HDR;

            v2f vert (appdata v)
            {
                v2f o = (v2f)0;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 n = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld,v.vertex);
                o.worldPos = worldPos;
                o.normal = n;

                return o;
            }

            half4 BlurSample(float2 pos){
                const int2 offsets[4] = {-1,-1,-1,1, 1,1, 1,-1};
                float2 suv = pos / _ScaledScreenParams.xy;
                
                half4 c = tex2D(_ReflectionTexture, suv);
                for(int i=0;i<4;i++){
                    suv = (pos + offsets[i])/_ScaledScreenParams.xy;
                    c+= tex2D(_ReflectionTexture, suv);
                }
                return c*0.2;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 suv = i.vertex.xy / _ScaledScreenParams.xy;
                #if defined(UNITY_UV_STARTS_AT_TOP)
                // suv.y = 1 - suv.y;
                #endif
                // return float4(suv,0,0);
                // sample the texture
                float4 sspr = tex2D(_ReflectionTexture, suv);
// return sspr;
                float r = 1  - _Smoothness;
                float lod = (1.7-0.7*r)*6*r;
                float3 reflectDir = reflect(normalize(i.worldPos - _WorldSpaceCameraPos),i.normal);
                float3 cubeCol = DecodeHDR(texCUBElod(_IBL,float4(reflectDir,lod)),_IBL_HDR);
               
                return lerp(cubeCol,sspr.xyz,sspr.w).xyzx;
                // return BlurSample(i.vertex.xy);
            }
            ENDCG
        }
    }
}
