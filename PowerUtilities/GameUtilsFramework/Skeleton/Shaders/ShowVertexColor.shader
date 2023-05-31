Shader "Hidden/PowerUtilities/Unlit/ShowVertexColor"
{
    Properties
    {
        _Alpha("_Alpha",range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "queue"="transparent"}
        LOD 100
        blend srcAlpha oneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../../../../../PowerShaderLib/Lib/MathLib.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color:COLOR;
            };
            float _Alpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // OffsetHClipVertexZ(o.vertex/**/,-0.1);
                #if UNITY_REVERSED_Z
                o.vertex.z += 0.0001;
                #else
                o.vertex.z -= 0.0001;
                #endif
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
