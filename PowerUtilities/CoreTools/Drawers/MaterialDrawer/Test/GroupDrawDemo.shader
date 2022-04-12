Shader "Unlit/GroupDrawDemo"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        //show a new Group
        [Group(group1,true)] _NewGroup("_NewGroup",int) = 0
        [Group(group1)]_MainTex2 ("Texture2", 2D) = "white" {}
        // show group item
        [Group(group1)]_FloatVlaue("_FloatVlaue",range(0,1)) = 0.1
        
        // show Toggle
        [GroupToggle(group1)]_ToggleNoKeyword("_ToggleNoKeyword",int) = 1
        [GroupToggle(group1,_Ker)]_ToggleWithKeyword("_ToggleWithKeyword",int) = 1


        [GroupHeader(group1,b)]
        // show Enum
        [GroupEnum(group1, _kEYA _KEYB,true)]_GroupKeywordEnum("_GroupKeywordEnum",int) = 0
        [GroupEnum(group1,A 0 B 1)]_GroupEnum("_GroupEnum",int) = 0
    }

}
