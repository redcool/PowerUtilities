#if UNITY_EDITOR
namespace PowerUtilities
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draw PresetBlendMode in GroupAPI
    /// 
    /// 
    /*  
     *  shader demo:
        [GroupHeader(Alpha,BlendMode)]
        [GroupPresetBlendMode(Alpha,, _SrcMode, _DstMode)] _PresetBlendMode("_PresetBlendMode", int)=0
        [HideInInspector] _SrcMode("_SrcMode", int) = 1
        [HideInInspector] _DstMode("_DstMode", int) = 0
    */
    /// </summary>
    public class GroupPresetBlendModeDrawer : BaseGroupItemDrawer
    {
        string srcModeName, dstModeName;
        public GroupPresetBlendModeDrawer(string groupName, string tooltip,string srcModeName,string dstModeName) : base(groupName, tooltip)
        {
            this.srcModeName = srcModeName;
            this.dstModeName = dstModeName;
        }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            var mat = editor.target as Material;
            //EditorGUI.DrawRect(position, Color.red);
            MaterialEditorGUITools.DrawBlendMode(position, mat, "PresetBlendMode", srcModeName, dstModeName);
        }
    }
}
#endif