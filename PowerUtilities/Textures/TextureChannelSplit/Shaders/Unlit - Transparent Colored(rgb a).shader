Shader "Unlit/Transparent Colored (rgb+a)"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "black" {}
		_AlphaTex("Alpha (A)",2d) = ""{}
		[KeywordEnum(R,A)]_AlphaTexChannel("AlphaTexChannel",float) = 0
	}
	
	SubShader
	{
		LOD 200

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"DisableBatching" = "True"
		}
		
		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			Offset -1, -1
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag			
			#include "UnityCG.cginc"
			#pragma multi_compile _ALPHATEXCHANNEL_R _ALPHATEXCHANNEL_A

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _AlphaTex;
	
			struct appdata_t
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
	
			struct v2f
			{
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
				UNITY_VERTEX_OUTPUT_STEREO
			};
	

			v2f vert (appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				o.color = v.color;
				return o;
			}
				
			fixed4 frag (v2f IN) : SV_Target
			{
				float4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
				c.a = tex2D(_AlphaTex, IN.texcoord).a;

#if _ALPHATEXCHANNEL_R
				c.a = tex2D(_AlphaTex, IN.texcoord).r;
#endif
				return c;
			}
			ENDCG
		}
	}

}
