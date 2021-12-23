Shader "Unlit/ColorTransform"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Saturate("Saturrate",float) = 0.5
		_Luminance("Luminance",float) = 0.5
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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			float _Saturate;
			float _Luminance;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				fixed3 gray = dot(fixed3(0.2, 0.7, 0.02), col.rgb);
				fixed3 saturate = lerp(gray,col.rgb,_Saturate); //饱和度
				fixed3 bright = lerp(fixed3(0,0,0),col.rgb, _Luminance);	//亮度

				return float4(saturate + bright, 1);
			}
            ENDCG
        }
    }
}
