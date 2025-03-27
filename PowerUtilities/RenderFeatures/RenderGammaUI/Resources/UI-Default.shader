// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/Default"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        //===================
        [Group(Color)]
        [GroupItem(Color)] _Color ("Tint", Color) = (1,1,1,1)

        [GroupEnum(Color,RGBA 16 RGB 15 RG 12 GB 6 RB 10 R 8 G 4 B 2 A 1 None 0)] _ColorMask ("Color Mask", Float) = 15
        //===================
        [Group(Effects)]
        [GroupToggle(Effects)]_GrayOn("_GrayOn",int) = 0
        //===================
        [Group(Alpha)]
        [GroupHeader(Alpha,Clip)]
        [GroupToggle(Alpha,UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        [GroupEnum(Alpha,R 0 G 1 B 2 A 3)] _AlphaChannel("_AlphaChannel",int) = 3

        [GroupHeader(Alpha,Glyph)]
        [GroupToggle(Alpha,_GLYPH_ON)]_GlyphOn("_GlyphOn",float) = 0
        [GroupVectorSlider(Alpha,min max,0_1 0_1,glyph edge smooth)] _GlyphRange("_GlyphRange",vector) = (0.1,0.5,0,0)

        [GroupHeader(Alpha,Blend)]
        [GroupPresetBlendMode(Alpha,,_SrcMode,_DstMode)]_PresetBlendMode("_PresetBlendMode",int)=0
        // [GroupEnum(Alpha,UnityEngine.Rendering.BlendMode)]
        [HideInInspector]_SrcMode("_SrcMode",int) = 1
        [HideInInspector]_DstMode("_DstMode",int) = 10  
        //===================
        [Group(Settings)]
		[GroupToggle(Settings)]_ZWriteMode("ZWriteMode",int) = 0
		/*
		Disabled,Never,Less,Equal,LessEqual,Greater,NotEqual,GreaterEqual,Always
		*/
		[GroupEnum(Settings,UnityEngine.Rendering.CompareFunction)]_ZTestMode("_ZTestMode",float) = 8
        [GroupEnum(Settings,UnityEngine.Rendering.CullMode)]_CullMode("_CullMode",int) = 0
        //===================
        [Group(Stencil)]
        [GroupEnum(Stencil,UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 0
        [GroupStencil(Stencil)] _Stencil ("Stencil ID", int) = 0
        [GroupEnum(Stencil,UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Float) = 0
        [GroupEnum(Stencil,UnityEngine.Rendering.StencilOp)] _StencilFailOp ("Stencil Fail Operation", Float) = 0
        [GroupItem(Stencil)] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [GroupItem(Stencil)] _StencilReadMask ("Stencil Read Mask", Float) = 255
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

         Stencil
         {
             Ref [_Stencil]
             Comp [_StencilComp]
             Pass [_StencilOp]
             Fail[_StencilFailOp]
             ReadMask [_StencilReadMask]
             WriteMask [_StencilWriteMask]
         }

        Cull[_CullMode]
        Lighting Off
        ZWrite [_ZWriteMode]
        ZTest [unity_GUIZTestMode]
        // Blend One OneMinusSrcAlpha
        blend [_SrcMode][_DstMode]
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            // #include "UnityCG.cginc"
            #include "../../../../../PowerShaderLib/Lib/UnityLib.hlsl"
            #include "UnityUI.cginc"
            
            // gamma linear space
            #include "../../../../../PowerShaderLib/Lib/ColorSpace.hlsl"
            #pragma multi_compile_fragment _ _SRGB_TO_LINEAR_CONVERSION _LINEAR_TO_SRGB_CONVERSION

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma shader_feature _GLYPH_ON

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                half4  mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float2 _GlyphRange;
            half _GrayOn;
            half _AlphaChannel;
            CBUFFER_END

            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 vPosition = UnityObjectToClipPos(v.vertex);
                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;

                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                OUT.mask = half4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

                OUT.color = v.color * _Color;
                return OUT;
            }

            half4 frag(v2f IN) : SV_Target
            {
                float4 mainTex = tex2D(_MainTex, IN.texcoord);
                #if defined(_GLYPH_ON)
                mainTex.w = smoothstep(_GlyphRange.x,_GlyphRange.y,mainTex.w);
                #endif

                half4 color = IN.color * (mainTex + _TextureSampleAdd);


                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color[_AlphaChannel] - 0.001);
                #endif

                /**
                    1 DrawObjectsPass, enable _SRGB_TO_LINEAR_CONVERSION
                    2 texture enable SRGB
                **/
			    LinearGammaAutoChange(color/**/);

                color.xyz = lerp(color.xyz,dot(float3(0.2,0.7,0.02),color.xyz),_GrayOn);
                return color;
            }
        ENDHLSL
        }
    }
}
