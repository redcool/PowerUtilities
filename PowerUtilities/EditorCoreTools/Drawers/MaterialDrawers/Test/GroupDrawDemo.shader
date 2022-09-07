Shader "Unlit/GroupDrawDemo"
{
    Properties
    {
        [Tooltip(show float value)]
        Test_FloatVlaue("_FloatVlaue0",range(0,1)) = 0.1
        
        [Group(group0)]
        [GroupHeader(group0,header0,header0 helps)]
        [GroupHeader(group0,header0,header0 helps)]
        [GroupHeader(group0,header0,header0 helps)]

        [GroupItem(group0,_FloatVlaue0 helps)]
        _FloatVlaue0("_FloatVlaue0",range(0,1)) = 0.1

        [GroupItem(group0,_FloatVlaue0 helps)]_FloatVlaue0_1("_FloatVlaue0_1",range(0,1)) = 0.1
        [GroupItem(group0,_FloatVlaue0 helps)]_FloatVlaue0_2("_FloatVlaue0_1",range(0,1)) = 0.1
        [GroupItem(group0,_FloatVlaue0 helps)]_FloatVlaue0_3("_FloatVlaue0_1",range(0,1)) = 0.1
        [GroupItem(group0,_FloatVlaue0 helps)]_FloatVlaue0_4("_FloatVlaue0_1",range(0,1)) = 0.1
        [GroupItem(group0,_FloatVlaue0 helps)]_FloatVlaue0_5("_FloatVlaue0_1",range(0,1)) = 0.1
        [GroupItem(group0,_FloatVlaue0 helps)]_FloatVlaue0_6("_FloatVlaue0_1",range(0,1)) = 0.1
[GroupItem(group0,_MainTex0 helps)]_MainTex0 ("Texture0", 2D) = "white" {}
        // //show a new Group
        [Group(group1)]
        [GroupItem(group1,_MainTex1 helps)]_MainTex1 ("Texture1", 2D) = "white" {}
        // // show group item
        [GroupItem(group1,_FloatVlaue1 helps)]_FloatVlaue("_FloatVlaue1",range(0,1)) = 0.1
        // remap slider
        [GroupSlider(group1,_GroupSlider helps)]_GroupSlider("_GroupSlider",range(0.1,0.5)) = 0.2
        [GroupItem(group1)]_FloatVlaue2("_FloatVlaue2",float) = 0.1
        // Toggle
        [GroupToggle(group1)]_ToggleNoKeyword("_ToggleNoKeyword",int) = 1
        [GroupToggle(group1,_Ker,_ToggleWithKeyword helps)]_ToggleWithKeyword("_ToggleWithKeyword",int) = 1
        //header
        [GroupHeader(group1,header1,header1 helps)]
        // show Enum with keyword
        [GroupEnum(group1 ,_kEYA _KEYB,true,_GroupKeywordEnum helps)]_GroupKeywordEnum("_GroupKeywordEnum",int) = 0
        // // show Enum, space is splitter 
        [GroupEnum(group1,A 0 B 1)]_GroupEnum("_GroupEnum",int) = 0
        [GroupEnum(group1,UnityEngine.Rendering.BlendMode)]_GroupEnumBlend("_GroupEnumBlend",int) = 0

        // vector slider
        [GroupVectorSlider(group1,a b c d,0_1 1_2 0_1 0_2)] _Vector("_Vector",vector) = (1,1,1,1)
        [GroupVectorSlider(group1,Dir(xyz) intensity, 0_1)]_Vector2("_Vector2", vector) = (1,0.1,0,1)
        [GroupVectorSlider(group1,a b,0_1 1_2,_Vector3 helps)] _Vector3("_Vector3",vector) = (1,1,1,1)

        [Group(group2)]
        [GroupItem(group2,_MainTex2 help)]_MainTex2("Texture2", 2D) = "white" {}
    }
    SubShader{
        pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}