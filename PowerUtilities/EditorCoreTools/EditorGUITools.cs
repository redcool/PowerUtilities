#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    public static class EditorGUITools
    {
        public static Color darkGray = new Color(0.2f, 0.3f, 0.4f);

        static GUIContent tempContent = new GUIContent();
        public static GUIContent TempContent(string str,string tooltip)
        {
            tempContent.text = str;
            tempContent.tooltip = tooltip;
            return tempContent;
        }

        public static void DrawPreview(Texture tex)
        {
            var rect = GUILayoutUtility.GetRect(100, 100, "Box");
            EditorGUI.DrawPreviewTexture(rect, tex);
        }


        public static void DrawSplitter(float w, float h)
        {
            GUILayout.Box("", GUILayout.Height(h), GUILayout.Width(w));
        }

        #region Normal UI,Label
        public static void DrawFixedWidthLabel(float width, Action drawAction)
        {
            if (drawAction == null)
                return;

            var lastLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = width;
            drawAction();
            EditorGUIUtility.labelWidth = lastLabelWidth;
        }

        public static void DrawColorLabel(string label, Color color)
        {
            var c = GUI.color;
            GUI.color = color;
            EditorGUILayout.LabelField(label);
            GUI.color = c;
        }

        public static void DrawColorUI(Action drawAction, Color contentColor, Color color)
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

        public static float DrawRemapSlider(Rect position, Vector2 range, GUIContent label, float value)
        {
            float v = EditorGUI.Slider(position, label, Mathf.InverseLerp(range.x, range.y, value), 0, 1);
            return Mathf.Lerp(range.x, range.y, v);
        }

        #endregion

        #region Box And Group

        public static void BeginVerticalBox(Action drawAction, string style = "Box")
        {
            if (drawAction == null)
                return;

            EditorGUILayout.BeginVertical(style);
                drawAction();
            EditorGUILayout.EndVertical();
        }
        public static void BeginHorizontalBox(Action drawAction, string style = "Box")
        {
            if (drawAction == null)
                return;

            EditorGUILayout.BeginHorizontal(style);
                drawAction();
            EditorGUILayout.EndHorizontal();
        }

        public static void BeginDisableGroup(bool isDisabled, Action drawAction)
        {
            if (drawAction == null)
                return;

            EditorGUI.BeginDisabledGroup(isDisabled);
            drawAction();
            EditorGUI.EndDisabledGroup();
        }

        #endregion

        public static int LayerMaskField(string label, int layers)
        {
            var tempLayers = InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layers);
            tempLayers = EditorGUILayout.MaskField(label, tempLayers, InternalEditorUtility.layers);
            return InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempLayers);
        }


        #region Foldout
        public static void DrawFoldContent(ref (string title, bool fold) foldInfo, Action drawContentAction)
        {
            DrawFoldContent(ref foldInfo, drawContentAction, GUI.contentColor);
        }

        public static void DrawFoldContent(ref (string title, bool fold) foldInfo, Action drawContentAction, Color titleColor, float space = 1)
        {
            var lastColor = GUI.contentColor;
            GUI.contentColor = titleColor;

            var lastBg = GUI.backgroundColor;
            GUI.backgroundColor = Color.gray;

            // draw title button bar
            EditorGUILayout.BeginVertical("Button");
            foldInfo.fold = EditorGUILayout.Foldout(foldInfo.fold, foldInfo.title, true);
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

        #endregion

        #region Additional Controls

        /// <summary>
        /// 
        /// Hold control select multi buttons.
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="toggles"></param>
        /// <param name="selectedIds"></param>
        /// <param name="xCount"></param>
        /// <param name="rowStyle"></param>
        /// <param name="columnStyle"></param>
        /// <returns></returns>
        public static bool MultiSelectionGrid(GUIContent[] contents, bool[] toggles, List<int> selectedIds, int xCount, GUIStyle rowStyle = null, GUIStyle columnStyle = null)
        {
            var e = Event.current;
            // check styles
            rowStyle = rowStyle == null ? GUIStyle.none : rowStyle;
            columnStyle = columnStyle == null ? GUIStyle.none : columnStyle;

            //calc rows
            var rows = contents.Length / xCount;
            if (contents.Length % xCount != 0)
                rows++;

            // item
            var itemIndex = 0;
            var inspectorWidth = EditorGUIUtility.currentViewWidth;
            var itemWidth = (inspectorWidth - 4*xCount -40) / xCount ; // guess inspector's width
            var hasChanged = false;

            GUILayout.BeginVertical(columnStyle);
            for (int x = 0; x < rows; x++)
            {
                GUILayout.BeginHorizontal(rowStyle);
                for (int y = 0; y < xCount; y++)
                {
                    itemIndex = y + x * xCount;
                    if (itemIndex >= toggles.Length)
                        break;

                    var lastToggle = toggles[itemIndex];
                    toggles[itemIndex] = GUILayout.Toggle(lastToggle, contents[itemIndex], "Button", GUILayout.Width(itemWidth));


                    if(lastToggle != toggles[itemIndex])
                    {
                        SelectionControl(toggles, selectedIds, e, itemIndex);
                    }
                }
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();

            return hasChanged;

            // inner method
            void SelectionControl(bool[] toggles, List<int> selectedIds, Event e, int itemIndex)
            {
                // multiple selection
                if (e.control)
                {
                    var isSelected = toggles[itemIndex];
                    var isContained = selectedIds.Contains(itemIndex);

                    if (isSelected && !isContained)
                        selectedIds.Add(itemIndex);

                    if (!isSelected && isContained)
                        selectedIds.Remove(itemIndex);
                }
                else
                {
                    // single selection
                    for (int i = 0; i < toggles.Length; i++)
                    {
                        if (i != itemIndex)
                            toggles[i] = false;
                    }

                    selectedIds.Clear();
                    selectedIds.Add(itemIndex);
                }
            }
        }

        public static bool MultiSelectionGrid(string[] contents, bool[] toggles, List<int> selectedIds, int xCount, GUIStyle rowStyle = null, GUIStyle columnStyle = null)
        {
            var guiContents = contents.Select(str => new GUIContent(str)).ToArray();
            return MultiSelectionGrid(guiContents, toggles, selectedIds, xCount, rowStyle, columnStyle);
        }

        /// <summary>
        /// Draw Header with (toggle, fold)
        /// like:
        ///     (bool isCheck, bool isFold) foldInfo = (false, false);
        ///     EditorGUITools.ToggleFoldoutHeader(position, ref foldInfo.isFold, ref foldInfo.isCheck, groupName);
        /// </summary>
        /// <param name="position"></param>
        /// <param name="isFolded"></param>
        /// <param name="isChecked"></param>
        /// <param name="label"></param>
        public static void ToggleFoldoutHeader(Rect position,ref bool isFolded,ref bool isChecked,string label)
        {
            //var lineRect = new Rect(position.x, position.y, position.width, 24);
            
            GUI.Box(position, label, EditorStylesEx.ShurikenModuleTitle);

            var checkRect = new Rect(position.x, position.y+2, 16, position.height);
            var e = Event.current;
            if(e.type == EventType.Repaint)
            {
                EditorStylesEx.ShurikenCheckMark.Draw(checkRect, "", true, true, isChecked, false);

            }
            if (e.type == EventType.MouseDown)
            {
                if (checkRect.Contains(e.mousePosition))
                {
                    isChecked = !isChecked;
                    e.Use();
                }
                else if (position.Contains(e.mousePosition))
                {
                    isFolded = !isFolded;
                    e.Use();
                }

            }
        }

        #endregion

        public static void DrawPropertyEditor(SerializedProperty settingsProp, ref bool isSettingsFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical("Box");
            {
                isSettingsFoldout = EditorGUILayout.Foldout(isSettingsFoldout, settingsProp.displayName,true);
                if (isSettingsFoldout)
                {

                    var settingsEditor = Editor.CreateEditor(settingsProp.objectReferenceValue);
                    settingsEditor.DrawDefaultInspector();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
    }
}
#endif