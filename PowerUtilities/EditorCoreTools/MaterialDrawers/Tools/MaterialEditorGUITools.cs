#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;

    public static class MaterialEditorGUITools
    {
        public static void DrawField(Material mat, string header, Action onGUI, Action<Material> onGUIChanged = null)
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical();
            EditorGUILayout.Space(10);
            GUILayout.Label(header, EditorStyles.boldLabel);
            if (onGUI != null)
                onGUI();

            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                if (onGUIChanged != null)
                    onGUIChanged(mat);
            }
        }

        public static void DrawBlendMode(Rect pos,Material mat, string propName, ref PresetBlendMode presetBlendMode
            , string srcModeName = PresetBlendModeTools.SRC_MODE
            , string dstModeName = PresetBlendModeTools.DST_MODE)
        {
            EditorGUI.BeginChangeCheck();
            presetBlendMode = (PresetBlendMode)EditorGUI.EnumPopup(pos,propName, presetBlendMode);
            if (EditorGUI.EndChangeCheck())
            {
                var blendModes = PresetBlendModeTools.blendModeDict[presetBlendMode];
                mat.SetFloat(srcModeName, (int)blendModes[0]);
                mat.SetFloat(dstModeName, (int)blendModes[1]);
            }
        }

        public static void DrawBlendMode(Material mat, string propName, ref PresetBlendMode presetBlendMode
            ,string srcModeName=PresetBlendModeTools.SRC_MODE
            ,string dstModeName=PresetBlendModeTools.DST_MODE)
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical();
            EditorGUILayout.Space(10);
            GUILayout.Label("Alpha Blend", EditorStyles.boldLabel);

            presetBlendMode = (PresetBlendMode)EditorGUILayout.EnumPopup(propName, presetBlendMode);

            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                var blendModes = PresetBlendModeTools.blendModeDict[presetBlendMode];
                mat.SetFloat(srcModeName, (int)blendModes[0]);
                mat.SetFloat(dstModeName, (int)blendModes[1]);
            }
        }
    }
}
#endif