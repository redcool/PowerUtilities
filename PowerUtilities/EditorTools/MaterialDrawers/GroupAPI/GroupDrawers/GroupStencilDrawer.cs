#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;
    using UnityEngine.Rendering;

    /// <summary>
    /// Material's sterncil Attribute
    /// define stencil value : ProjectSettings/PowerUtils/Project/StencilPreset
    /// </summary>
    public class GroupStencilDrawer : BaseGroupItemDrawer
    {
        readonly GUIContent DROPDOWN_TOOLTIPS = new GUIContent("¡ý", "show predefine stencil values");
        public GroupStencilDrawer() : base("",null) {}
        public GroupStencilDrawer(string groupName) : base(groupName, null) { }
        public GroupStencilDrawer(string groupName,string tooltip) : base(groupName,tooltip) { }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            var intFieldPos = position;
            intFieldPos.width -= 20;
            prop.floatValue = (int)EditorGUI.FloatField(intFieldPos, label, prop.floatValue);

            var dropdownPos = new Rect(position.xMax - 17, position.y, 18, 18);
            var isClicked = GUI.Button(dropdownPos, DROPDOWN_TOOLTIPS);
            if (isClicked)
            {
                var stencilSettingSo = ScriptableObjectTools.CreateGetInstance<StencilSetting>();
                var itemList = stencilSettingSo.itemList
                    .Select(item =>($"{item.name} , {item.value}",(object)item))
                    .ToList();

                SearchWindowTools.CreateStringListProviderAndShowWin("preset stencil ref", itemList, ((string name, object userData) item) =>
                {
                    var stencilId = ((StencilValueInfo)item.userData).value;
                    prop.floatValue = stencilId;
                });
            }
        }

    }

}
#endif