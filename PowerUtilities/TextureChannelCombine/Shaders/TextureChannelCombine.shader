Shader "Hidden/TextureChannelCombine"
{

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
                float4 vertex : SV_POSITION;
            };

            sampler2D _RTex;
			sampler2D _GTex;
			sampler2D _BTex;
			sampler2D _ATex;

			float4 _RTexMask;
			float4 _GTexMask;
			float4 _BTexMask;
			float4 _ATexMask;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed r = dot(tex2D(_RTex, i.uv),_RTexMask);
				fixed g = dot(tex2D(_GTex, i.uv),_GTexMask);
				fixed b = dot(tex2D(_BTex, i.uv),_BTexMask);
				fixed a = dot(tex2D(_ATex, i.uv),_ATexMask);
                return fixed4(r,g,b,a);
            }
            ENDCG
        }
    }
}
