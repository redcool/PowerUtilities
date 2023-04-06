Shader "Unlit/ShowReflectionTexture"
{
    Properties
    {
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _ScreenSpacePlanarReflectionTexture;
            float4 _ScaledScreenParams;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
// o.vertex = float4(v.vertex.xyz*2,1);
                return o;
            }

            half4 BlurSample(float2 pos){
                const int2 offsets[4] = {-1,-1,-1,1, 1,1, 1,-1};
                float2 suv = pos / _ScaledScreenParams.xy;
                
                half4 c = tex2D(_ScreenSpacePlanarReflectionTexture, suv);
                for(int i=0;i<4;i++){
                    suv = (pos + offsets[i])/_ScaledScreenParams.xy;
                    c+= tex2D(_ScreenSpacePlanarReflectionTexture, suv);
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
                return tex2D(_ScreenSpacePlanarReflectionTexture, suv);
                // return BlurSample(i.vertex.xy);
            }
            ENDCG
        }
    }
}
