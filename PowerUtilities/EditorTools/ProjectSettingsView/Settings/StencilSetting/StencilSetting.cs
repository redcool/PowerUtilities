namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;



    [Serializable]
    public class StencilValueInfo
    {
        [Tooltip("stencil ref value")]
        public int value;

        [Tooltip("stencil name ")]
        public string name;

        [Tooltip("stencil description")]
        public string desc;
    }

    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Project/StencilSetting", isUseUIElment =false)]
    [SOAssetPath("Assets/PowerUtilities/StencilSetting.asset")]
    public class StencilSetting : ScriptableObject
    {
        [HelpBox]
        public string helpBox = "define stencil id and desc, shader like : [GroupStencil(Stencil)] _Stencil (\"Stencil ID\", int) = 0";
        [ListItemDraw("name:,name,value:,value,desc:,desc","50,100,50,100,50,")]
        public List<StencilValueInfo> itemList = new ();
    }

}