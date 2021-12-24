#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    public static class EditorGUITools
    {
        public static Color darkGray = new Color(0.2f,0.3f,0.4f);
        public static void DrawPreview(Texture tex)
        {
            var rect = GUILayoutUtility.GetRect(100, 100, "Box");
            EditorGUI.DrawPreviewTexture(rect, tex);
        }

        public static void DrawSplitter(float w, float h)
        {
            GUILayout.Box("", GUILayout.Height(h), GUILayout.Width(w));
        }

        public static void DrawFixedWidthLabel(float width, Action drawAction)
        {
            if (drawAction == null)
                return;

            var lastLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = width;
            drawAction();
            EditorGUIUtility.labelWidth = lastLabelWidth;
        }

        public static void BeginVerticalBox(Action drawAction,string style="Box")
        {
            EditorGUILayout.BeginVertical(style);
            if (drawAction != null)
                drawAction();
            EditorGUILayout.EndVertical();
        }
        public static void BeginHorizontalBox(Action drawAction, string style = "Box")
        {
            EditorGUILayout.BeginHorizontal(style);
            if (drawAction != null)
                drawAction();
            EditorGUILayout.EndHorizontal();
        }

        public static int LayerMaskField(string label, int layers)
        {
            var tempLayers = InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layers);
            tempLayers = EditorGUILayout.MaskField(label, tempLayers, InternalEditorUtility.layers);
            return InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempLayers);
        }

        public static void DrawFoldContent(ref (string title, bool fold) foldInfo, Action drawContentAction)
        {
            DrawFoldContent(ref foldInfo, drawContentAction, GUI.contentColor);
        }

        public static void DrawFoldContent(ref (string title, bool fold) foldInfo, Action drawContentAction, Color titleColor, float space=1)
        {
            var lastColor = GUI.contentColor;
            GUI.contentColor = titleColor;

            var lastBg = GUI.backgroundColor;
            GUI.backgroundColor = Color.gray;

            // draw title button bar
            EditorGUILayout.BeginVertical("Button");
            foldInfo.fold = EditorGUILayout.Foldout(foldInfo.fold, foldInfo.title,true);
            EditorGUILayout.Space(space);
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = lastBg;
            GUI.contentColor = lastColor;

            //draw content
            if (foldInfo.fold)
            {
                ++EditorGUI.indentLevel;
                drawContentAction();
                --EditorGUI.indentLevel;
            }
        }

        public static void DrawColorUI(Action drawAction,Color contentColor,Color color)
        {
            if (drawAction == null)
                return;

            var lastContentColor = GUI.contentColor;
            var lastColor = GUI.color;

            GUI.contentColor = contentColor;
            GUI.color = color;
            drawAction();

            GUI.contentColor = lastContentColor;
            GUI.color = lastColor;
        }
    }
}
#endif