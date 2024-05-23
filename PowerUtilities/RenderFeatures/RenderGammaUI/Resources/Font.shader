// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "GUI/Text Shader" {
    Properties {
        [PerRenderer]_MainTex ("Font Texture", 2D) = "white" {}
        _Color ("Text Color", Color) = (1,1,1,1)

        [Group(Alpha)]
        [GroupToggle(Alpha,_GLYPH_ON)]_GlyphOn("_GlyphOn",float) = 0
        [GroupVectorSlider(Alpha,min max,0_1 0_1,glyph edge smooth)] _GlyphRange("_GlyphRange",vector) = (0.1,0.5,0,0)
        [GroupPresetBlendMode(Alpha,,_SrcMode,_DstMode)]_PresetBlendMode("_PresetBlendMode",int)=0
        // [GroupEnum(Alpha,UnityEngine.Rendering.BlendMode)]
        [HideInInspector]_SrcMode("_SrcMode",int) = 5
        [HideInInspector]_DstMode("_DstMode",int) = 10

        [Group(Settings)]
		[GroupToggle(Settings)]_ZWriteMode("ZWriteMode",int) = 0
		/*
		Disabled,Never,Less,Equal,LessEqual,Greater,NotEqual,GreaterEqual,Always
		*/
		[GroupEnum(Settings,UnityEngine.Rendering.CompareFunction)]_ZTestMode("_ZTestMode",float) = 8
        [GroupEnum(Settings,UnityEngine.Rendering.CullMode)]_CullMode("_CullMode",int) = 0
    }

    SubShader {

        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        Lighting Off 
        Cull[_CullMode]
        ZTest [_ZTestMode]
        ZWrite [_ZWriteMode]
        // Blend SrcAlpha OneMinusSrcAlpha
        blend [_SrcMode][_DstMode]

        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #pragma multi_compile_fragment _ _SRGB_TO_LINEAR_CONVERSION _LINEAR_TO_SRGB_CONVERSION
            #pragma shader_feature _GLYPH_ON

            #include "../../../../../PowerShaderLib/Lib/UnityLib.hlsl"
            #include "../../../../../PowerShaderLib/Lib/ColorSpace.hlsl"

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            CBUFFER_START(UnityPerMaterial)
            sampler2D _MainTex;
            uniform float4 _MainTex_ST;
            uniform fixed4 _Color;
            float4 _GlyphRange;
            CBUFFER_END

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = i.color;
                float alpha = tex2D(_MainTex, i.texcoord).a;
                
                #if defined(_GLYPH_ON)
                alpha = smoothstep(_GlyphRange.x,_GlyphRange.y,alpha);
                #endif

                col.a *= alpha;
                return col;
            }
            ENDHLSL
        }
    }
}
