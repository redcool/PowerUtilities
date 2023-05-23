namespace PowerUtilities
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draw PresetBlendMode in GroupAPI
    /// </summary>
    public class GroupPresetBlendModeDrawer : BaseGroupItemDrawer
    {
        PresetBlendMode presetBlendMode;
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
            MaterialEditorGUITools.DrawBlendMode(position, mat, "PresetBlendMode", ref presetBlendMode, srcModeName, dstModeName);
        }
    }
}