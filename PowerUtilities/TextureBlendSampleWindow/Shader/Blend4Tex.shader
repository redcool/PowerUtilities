Shader "Hidden/Blend4Tex"
{
    Properties
    {
        _ControlMap ("Texture", 2D) = "white" {}
        _Splat0("_Splat0",2d) = ""{}
        _Splat1("_Splat1",2d) = ""{}
        _Splat2("_Splat2",2d) = ""{}
        _Splat3("_Splat3",2d) = ""{}
    }
    SubShader
    {

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _ControlMap, _Splat0,_Splat1,_Splat2,_Splat3;
            float4 _Splat0_ST,_Splat1_ST,_Splat2_ST,_Splat3_ST;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 controlMap = tex2D(_ControlMap, i.uv);
                float2 splat0UV = i.uv * _Splat0_ST.xy + _Splat0_ST.zw;
                float2 splat1UV = i.uv * _Splat1_ST.xy + _Splat1_ST.zw;
                float2 splat2UV = i.uv * _Splat2_ST.xy + _Splat2_ST.zw;
                float2 splat3UV = i.uv * _Splat3_ST.xy + _Splat3_ST.zw;

                half4 splat0 = tex2D(_Splat0,splat0UV);
                half4 splat1 = tex2D(_Splat1,splat1UV);
                half4 splat2 = tex2D(_Splat2,splat2UV);
                half4 splat3 = tex2D(_Splat3,splat3UV);
                
                return splat0 * controlMap.r + splat1 * controlMap.g + splat2 * controlMap.b + splat3 * controlMap.a;
            }
            ENDCG
        }
    }
}

