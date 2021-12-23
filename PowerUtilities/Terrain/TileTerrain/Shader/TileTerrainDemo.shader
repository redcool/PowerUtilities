Shader "Unlit/TileTerrain"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Control("_Control",2d) = ""{}
        _Splat0("_Splat0",2d) = ""{}
        _Splat1("_Splat1",2d) = ""{}
        _Splat2("_Splat2",2d) = ""{}
        _Splat3("_Splat3",2d) = ""{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _Control;
            sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
            float4 _Splat0_ST,_Splat1_ST,_Splat2_ST,_Splat3_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            #define TRANSFORM_TEX(texName,uv) uv.xy * texName##_ST.xy + texName##_ST.zw

            fixed4 frag (v2f i) : SV_Target
            {
                float4 controlMap = tex2D(_Control,i.uv);
                float4 splat0 = tex2D(_Splat0,TRANSFORM_TEX(_Splat0,i.uv));
                float4 splat1 = tex2D(_Splat1,TRANSFORM_TEX(_Splat1,i.uv));
                float4 splat2 = tex2D(_Splat2,TRANSFORM_TEX(_Splat2,i.uv));
                float4 splat3 = tex2D(_Splat3,TRANSFORM_TEX(_Splat3,i.uv));

                float4 col = splat0 * controlMap.x + splat1 * controlMap.y + splat2 * controlMap.z + splat3 * controlMap.w;
                return float4(col.xyz,1);
            }
            ENDCG
        }
    }
}
