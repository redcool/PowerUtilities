Shader "Unlit/TransparentUnlit"
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
        blend srcAlpha oneMinusSrcAlpha

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
            float4 _Color;
            half _ShowOpaque;
            half4 _ScaledScreenParams;
            sampler2D _CameraOpaqueTexture;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                if(_ShowOpaque){
                    return tex2D(_CameraOpaqueTexture,i.vertex.xy/_ScaledScreenParams.xy)*_Color;
                }
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
}
