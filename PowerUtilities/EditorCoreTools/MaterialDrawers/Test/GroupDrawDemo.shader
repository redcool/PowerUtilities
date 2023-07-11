Shader "Unlit/GroupDrawDemo"
{
    Properties
    {
        [DocumentURL(123)]
        _HelpURL("http://www.baidu.com",int) = 0


        [Tooltip(show float value)]
        Test_FloatValue("_FloatValue0",range(0,1)) = 0.1
// show toggle group        
        [Group(ToggleGroup,ToggleGroup helps,false,_KEY1 _KEY2,#00ff0080)]
        [GroupHeader(ToggleGroup,toggle group,header0 helps)]
[GroupSpace(ToggleGroup,5,#ff000040)]
        [GroupItem(ToggleGroup,_FloatValue0 helps)]        
        _FloatValue0("_FloatValue0",range(0,1)) = 0.1

        [GroupItem(ToggleGroup,_MainTex0 helps)]_MainTex0 ("Texture0", 2D) = "white" {}

//show GroupItems
[Group(Group)]
        [GroupHeader(Group,GroupItem_Texture)]
        [GroupItem(Group,_MainTex1 helps)]_MainTex1 ("Texture1", 2D) = "white" {}

        // // show group item
        [GroupHeader(Group,GroupItem_Float)]
        [GroupItem(Group,_FloatValue1 helps)]_FloatValue("_FloatValue1",range(0,1)) = 0.1

        [GroupMinMaxSlider(Group)]
        _VectorValue0("_VectorValue0",vector) = (0,0,0,1)

[Group(GroupSlider)]
        [GroupHeader(GroupSlider,float)]
        [GroupSlider(GroupSlider,_FloatValue0 helps,float)]_GroupSlider_float("float slider",range(0,2)) = 0.1

        [GroupHeader(GroupSlider,int)]
        [GroupSlider(GroupSlider,_FloatValue0 helps,int)]_GroupSlider_int("int slider",range(0,10)) = 0.1

        [GroupHeader(GroupSlider,remap(slider value is x,show range 0_1))]
        [GroupSlider(GroupSlider,_GroupSlider helps)]_GroupSliderRemap("remap slider",range(0.1,0.5)) = 0.2

 [Group(GroupToggle)]
        // Toggle
        [GroupHeader(GroupToggle,_ToggleNoKeyword)]
        [GroupToggle(GroupToggle)]_ToggleNoKeyword("_ToggleNoKeyword",int) = 1

        [GroupHeader(GroupToggle,_ToggleWithKeyword)]
        [GroupToggle(GroupToggle,_Ker,_ToggleWithKeyword helps)]_ToggleWithKeyword("_ToggleWithKeyword",int) = 1

        [Group(GroupEnum)]
        
        // show Enum with keyword
        [GroupHeader(GroupEnum,show Enum with keyword)]
        [GroupEnum(GroupEnum ,_kEYA _KEYB,true,_GroupKeywordEnum helps)]_GroupKeywordEnum("_GroupKeywordEnum",int) = 0

        // // show Enum, space is splitter 
        [GroupHeader(GroupEnum,show Enum, space is splitter )]
        [GroupEnum(GroupEnum,A 0 B 1)]_GroupEnum("_GroupEnum",int) = 0

        [GroupHeader(GroupEnum,show Enum, enum class)]
        //show Enum, enum class
        [GroupEnum(GroupEnum,UnityEngine.Rendering.BlendMode)]_GroupEnumBlend("_GroupEnumBlend",int) = 0

        // vector slider
        [Group(GroupVector)]
        [GroupVectorSlider(GroupVector,a b c d,0_1 1_2 0_1 0_m.2,helps,float)] _Vector("Vector4",vector) = (1,1,1,1)
        [GroupVectorSlider(GroupVector,Dir(xyz) intensity, 0_1)]_Vector2("Vector3,Slider1", vector) = (1,0.1,0,1)
        [GroupVectorSlider(GroupVector,a b,0_1 1_2,_Vector3 helps)] _Vector3("Vector2 float",vector) = (1,1,1,1)
        [GroupVectorSlider(GroupVector,a b,0_5 1_10,_Vector3 helps,int)] _Vector4("Vector2 Int",vector) = (1,1,1,1)
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
