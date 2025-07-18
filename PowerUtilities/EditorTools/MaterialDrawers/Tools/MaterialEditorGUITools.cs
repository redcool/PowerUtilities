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

        public static void DrawBlendMode(Rect pos, Material mat, GUIContent label
            , string srcModeName = PresetBlendModeTools.SRC_MODE
            , string dstModeName = PresetBlendModeTools.DST_MODE)
        {

            var presetBlendMode = PresetBlendModeTools.GetPresetBlendMode(mat);

            EditorGUI.BeginChangeCheck();
            presetBlendMode = (PresetBlendMode)EditorGUI.EnumPopup(pos, label, presetBlendMode);
            if (EditorGUI.EndChangeCheck())
            {
                var blendModes = PresetBlendModeTools.GetBlendMode(presetBlendMode);
                mat.SetFloat(srcModeName, (int)blendModes[0]);
                mat.SetFloat(dstModeName, (int)blendModes[1]);
            }
        }

        public static void DrawBlendMode(Material mat, GUIContent label
            , string srcModeName = PresetBlendModeTools.SRC_MODE
            , string dstModeName = PresetBlendModeTools.DST_MODE)
        {
            DrawBlendMode(mat, label, GUI.contentColor, srcModeName, dstModeName);
        }

        public static void DrawBlendMode(Material mat, GUIContent label
        , Color contentColor
        , string srcModeName = PresetBlendModeTools.SRC_MODE
        , string dstModeName = PresetBlendModeTools.DST_MODE)
        {
            var presetBlendMode = PresetBlendModeTools.GetPresetBlendMode(mat);

            GUILayout.BeginVertical();
            EditorGUILayout.Space(10);

            var lastColor = GUI.contentColor;
            GUI.contentColor = contentColor;
            //------------
            GUILayout.Label("Alpha Blend", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            presetBlendMode = (PresetBlendMode)EditorGUILayout.EnumPopup(label, presetBlendMode);

            if (EditorGUI.EndChangeCheck())
            {
                var blendModes = PresetBlendModeTools.GetBlendMode(presetBlendMode);
                mat.SetFloat(srcModeName, (int)blendModes[0]);
                mat.SetFloat(dstModeName, (int)blendModes[1]);
            }
            //------------
            GUI.contentColor = lastColor;
            GUILayout.EndVertical();
        }
    }
}
#endif