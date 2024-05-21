Shader "SFC/ShowOverdrawAdd"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("_Color",color) = (1,1,1,0.2)
        [Toggle]_ShowOpaque("_ShowOpaque",int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "queue"="transparent"}
        LOD 100

        Pass
        {
            blend one one

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
            
            half4 _ScaledScreenParams;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return 0.1;
            }
            ENDCG
        }
    }
}
