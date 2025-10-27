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
    using UnityEngine.UIElements;
    using UnityEngine.Internal;

    public static class EditorGUITools
    {
        public static Color darkGray = new Color(0.2f, 0.3f, 0.4f);
        public static int defaultLabelWidth = 200;

        /// <summary>
        /// editor gui a row height :18
        /// </summary>
        public static float singleLineHeight => EditorGUIUtility.singleLineHeight;
        /// <summary>
        /// editor gui rows height gap : 2
        /// </summary>
        public static float lineHeightGap => 2;
        /// <summary>
        /// editor gui a row total height : (height + gap(
        /// </summary>
        public static float lineHeightTotal => singleLineHeight + lineHeightGap;

        static float lastLabelWidth;



        [CompileFinished]
        public static void Init()
        {
            EditorGUIUtility.labelWidth = defaultLabelWidth;
        }
        /// <summary>
        /// set temporary guiContent
        /// </summary>
        /// <param name="str"></param>
        /// <param name="tooltip"></param>
        /// <param name="image"></param>
        /// <param name="inst"></param>
        /// <returns></returns>
        public static GUIContent TempContent(string str,string tooltip=default,Texture image = default,GUIContent inst = default)
            => GUIContentEx.TempContent(str, tooltip, image, inst);

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
            //var c = GUI.color;
            //GUI.color = color;
            //EditorGUILayout.LabelField(label);
            //GUI.color = c;
            DrawColorLabel(EditorGUILayout.GetControlRect(), TempContent(label), color);
        }
        public static void DrawColorLabel(Rect position,GUIContent label, Color color)
        {
            var c = GUI.color;
            GUI.color = color;
            EditorGUI.LabelField(position, label);
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

        public static void DrawIndent(Action drawAction,bool hasIndent,int indentLevelAdd=1)
        {
            if (drawAction == null)
                return;
            var intent = hasIndent ? indentLevelAdd : 0;
            EditorGUI.indentLevel += intent;
            drawAction();
            EditorGUI.indentLevel -= intent;
        }
        #region Box And Group

        public static void BeginVerticalBox(Action drawAction, string style = "Box",int indentLevelAdd=0)
        {
            if (drawAction == null)
                return;
            EditorGUI.indentLevel += indentLevelAdd;
            EditorGUILayout.BeginVertical(style);
                drawAction();
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel -= indentLevelAdd;
        }
        public static void BeginHorizontalBox(Action drawAction, string style = "Box")
        {
            if (drawAction == null)
                return;

            EditorGUILayout.BeginHorizontal(style);
                drawAction();
            EditorGUILayout.EndHorizontal();
        }

        public static T BeginVerticalBox<T>(Func<T> drawAction, string style = "Box")
        {
            if (drawAction == null)
                return default;

            EditorGUILayout.BeginVertical(style);
            var result = drawAction();
            EditorGUILayout.EndVertical();
            return result;
        }

        public static T BeginHorizontalBox<T>(Func<T> drawAction, string style = "Box")
        {
            if (drawAction == null)
                return default;

            EditorGUILayout.BeginHorizontal(style);
            var result = drawAction();
            EditorGUILayout.EndHorizontal();
            return result;
        }

        public static void BeginDisableGroup(bool isDisabled, Action drawAction)
        {
            if (drawAction == null)
                return;

            EditorGUI.BeginDisabledGroup(isDisabled);
            drawAction();
            EditorGUI.EndDisabledGroup();
        }

        public static void BeginFoldoutHeaderGroupBox(ref bool foldout, GUIContent content,Action drawActionWhenFoldout ,[DefaultValue("EditorStyles.foldoutHeader")] GUIStyle style = null, Action<Rect> menuAction = null, GUIStyle menuIcon = null)
        {
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, content, style, menuAction, menuIcon);
            if(foldout)
                drawActionWhenFoldout();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion

        public static int LayerMaskField(string label, int layers)
        {
            var tempLayers = InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layers);
            tempLayers = EditorGUILayout.MaskField(label, tempLayers, InternalEditorUtility.layers);
            return InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempLayers);
        }


        #region Foldout
        /// <summary>
        /// draw fold header, using in GUILayout 
        /// </summary>
        /// <param name="foldInfo"></param>
        /// <param name="drawContentAction"></param>
        public static void DrawFoldContent(ref (string title, bool fold) foldInfo, Action drawContentAction, Color titleColor=default, float space = 1)
        {
            DrawFoldContent(foldInfo.title, ref foldInfo.fold, drawContentAction, titleColor, space);
        }
        
        public static void DrawFoldContent(string title, ref bool fold, Action drawContentAction, Color titleColor=default, float space = 1)
            => DrawFoldContent(new GUIContent(title), ref fold, drawContentAction, titleColor, space);

        public static void DrawFoldContent(GUIContent title, ref bool fold, Action drawContentAction, Color titleColor=default, float space = 1)
        {
            var lastColor = GUI.contentColor;
            if(titleColor != default)
                GUI.contentColor = titleColor;

            var lastBg = GUI.backgroundColor;
            GUI.backgroundColor = Color.gray;

            // draw title button bar
            EditorGUILayout.BeginVertical(EditorStylesEx.box);
            fold = EditorGUILayout.Foldout(fold, title, true);
            EditorGUILayout.Space(space);
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = lastBg;
            GUI.contentColor = lastColor;

            //draw content
            if (fold)
            {
                ++EditorGUI.indentLevel;
                drawContentAction();
                --EditorGUI.indentLevel;
            }
        }

        /// <summary>
        /// Show (triangle,title ,style background)Foldout
        /// like (EditorGUILayout.BeginFoldoutHeaderGroup) 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="isOn"></param>
        /// <param name="title"></param>
        /// <param name="titleColor"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public static bool DrawTitleFoldout(Rect position, bool isOn, GUIContent title, Color titleColor, GUIStyle style,int lineHeight=22)
        {
            var pos = position;
            pos.height = lineHeight;

            // show only a style
            GUI.BeginGroup(pos, style);
            GUI.EndGroup();

            var isFold = EditorGUI.Foldout(pos, isOn, title, true);
            return isFold;
        }
        public static bool DrawTitleFoldout(Rect position, bool isOn, string title, Color titleColor, GUIStyle style)
        => DrawTitleFoldout(position, isOn, GUIContentEx.TempContent(title, ""), titleColor, style);

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
        /// <summary>
        /// draw tab window with multi selected
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="toggles"></param>
        /// <param name="selectedIds"></param>
        /// <param name="xCount"></param>
        /// <param name="rowStyle"></param>
        /// <param name="columnStyle"></param>
        /// <returns></returns>
        public static bool MultiSelectionGrid(string[] contents, bool[] toggles, List<int> selectedIds, int xCount, GUIStyle rowStyle = null, GUIStyle columnStyle = null)
        {
            var guiContents = contents.Select(str => new GUIContent(str)).ToArray();
            return MultiSelectionGrid(guiContents, toggles, selectedIds, xCount, rowStyle, columnStyle);
        }

        /// <summary>
        /// Draw Enum propertyField + enum searchable window
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="enumType"></param>
        public static void EnumPropertyFieldSearchable(SerializedProperty prop, Type enumType, Action<SerializedProperty, int> OnValueChanged=default)
        {
            if (OnValueChanged == null)
                OnValueChanged = DefulatValueChanged;

            EditorGUITools.BeginHorizontalBox(() =>
            {
                var labelWidth = EditorGUIUtility.labelWidth;

                EditorGUILayout.PrefixLabel(prop.displayName);
                var formatName = Enum.GetName(enumType, prop.intValue);
                if (GUILayout.Button(formatName, EditorStylesEx.DropDownButton))
                {
                    SearchWindowTools.CreateEnumProviderAndShowWin(prop, enumType, (id) =>
                    {
                        OnValueChanged(prop, id);
                    });
                }
            }, "");

            //------- inner methods
            static void DefulatValueChanged(SerializedProperty prop, int id)
            {
                prop.serializedObject.Update();
                prop.intValue = id;
                prop.serializedObject.ApplyModifiedProperties();
            }
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
        public static void ToggleFoldoutHeader(Rect position,GUIContent label,ref bool isFolded,ref bool isChecked)
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
                    GUI.changed = true;
                    e.Use();
                }
                else if (position.Contains(e.mousePosition))
                {
                    isFolded = !isFolded;
                    GUI.changed = true;
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

        public static string LabelTextField(GUIContent label,string defaultText="")
        {
            return BeginHorizontalBox<string>(() =>
            {
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                return EditorGUILayout.TextField(defaultText);
            });
        }

        public static void DrawPage(GUIContent label,Action onDrawDetail)
        {
            BeginVerticalBox(() =>
            {
                var pos = EditorGUILayout.GetControlRect();
                EditorGUILayout.LabelField(label, EditorStylesEx.BoldLabel);

                if (onDrawDetail != null)
                    onDrawDetail();

            },nameof(EditorStylesEx.GroupBox));
        }

        /// <summary>
        /// Draw default inspector,
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pos">occupy position(rect)</param>
        /// <returns></returns>
        public static bool DrawDefaultInspect(SerializedObject obj,out Rect pos)
        {
            // first pos(rect) is used to editor gui
            pos =  EditorGUILayout.GetControlRect(true,0);

            EditorGUI.BeginChangeCheck();
            var iterator = obj.GetIterator();
            var enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
                enterChildren = false;
                var lastRect = GUILayoutUtility.GetLastRect();
                // accumulation 
                pos.height += lastRect.height + EditorGUITools.lineHeightGap;
            }

            //DrawBoxColors(pos, backgroundColor: Color.red * 0.5f);

            return EditorGUI.EndChangeCheck();
        }

        /// <summary>
        /// box' color
        /// _________
        /// |       |
        /// |       |
        /// |       |
        /// _________
        /// </summary>
        /// <param name="position">ugui position</param>
        /// <param name="borderSize">edge size</param>
        /// <param name="backgroundColor"></param>
        /// <param name="leftColor"></param>
        /// <param name="topColor"></param>
        /// <param name="rightColor"></param>
        /// <param name="bottomColor"></param>
        public static void DrawBoxColors(Rect position, float borderSize = 2, Color backgroundColor = default, Color leftColor = default, Color topColor = default, Color rightColor = default, Color bottomColor = default)
        {
            var positions = new[] {
                position, // background rect
                new Rect(position.x, position.y, borderSize, position.height), // left edge
                new Rect(position.x,position.y,position.width,borderSize), // top edge
                new Rect(position.xMax-borderSize, position.y, borderSize, position.height), //right edge
                new Rect(position.x, position.yMax - borderSize, position.width, borderSize) //bottom edge
            };
            var colors = new[] {
                backgroundColor,leftColor, topColor, rightColor, bottomColor
            };
            colors.ForEach((color, id) =>
            {
                if (color != default)
                    EditorGUI.DrawRect(positions[id], color);
            });
        }

        public static void DrawColorLine(Rect pos, string colorStr = ColorTools.G_LIGHT_GREEN)
        {
            ColorUtility.TryParseHtmlString(colorStr, out var color);
            DrawBoxColors(pos, backgroundColor: color);
        }

        public static void DrawColorLine(int height =2, string colorStr = ColorTools.G_LIGHT_GREEN)
        {
            DrawColorLine(EditorGUILayout.GetControlRect(GUILayout.Height(height)), colorStr);
        }

        public static void DrawHelpURL(Rect position, string url)
        {
            var helpContent = EditorGUIUtility.IconContent("_Help");
            var buttonSize = position.height;
            var helpPos = new Rect(position.xMax-buttonSize, position.y, buttonSize, buttonSize);
            if (LinkButton(helpPos, helpContent))
            {
                Help.BrowseURL(url);
            }
        }

        public static void DrawTitleLabel(GUIContent label)
        {
            EditorGUILayout.LabelField(label, EditorStylesEx.titleStyle);
            EditorGUILayout.Space(5);
        }

        /// <summary>
        /// compatible for 2020
        /// </summary>
        /// <param name="position"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static bool LinkButton(Rect position, GUIContent label)
        {
#if UNITY_2020
            Handles.color = EditorStyles.linkLabel.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin + (float)EditorStyles.linkLabel.padding.left, position.yMax), new Vector3(position.xMax - (float)EditorStyles.linkLabel.padding.right, position.yMax));
            Handles.color = Color.white;
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            return GUI.Button(position, label, EditorStyles.linkLabel);
#else 
            return EditorGUI.LinkButton(position, label);
#endif
        }

        /// <summary>
        /// Show Button and confirm dialog
        /// </summary>
        /// <returns></returns>
        public static bool ButtonWithConfirm(string buttonText,string confirmText)
        {
            return GUILayout.Button(buttonText) && EditorTools.DisplayDialog_Ok_Cancel(confirmText);
        }

        #region SettingSO 



        /// <summary>
        /// Draw SettingSO(ScriptableObject) row
        /// </summary>
        /// <param name="settingSOProp"></param>
        /// <param name="targetEditor"></param>
        /// <param name="isTargetEditorFolded"></param>
        /// <param name="SettingSOType"></param>
        public static void DrawSettingSO(SerializedProperty settingSOProp, ref Editor targetEditor, ref bool isTargetEditorFolded, Type SettingSOType)
        {
            if (!ScriptableObjectEx.IsExtendsScriptableObject(SettingSOType))
            {
                Debug.LogError($"{SettingSOType} need extends ScriptableObject");
                return;
            }

            if (settingSOProp == null)
                return;

            //========================================  settingSO header
            DrawSettingSO(SettingSOType, settingSOProp);

            //========================================  splitter line 
            var rect = EditorGUILayout.GetControlRect(false, 2);
            EditorGUITools.DrawColorLine(rect);

            //========================================  draw settingSO detail 
            if (settingSOProp.objectReferenceValue != null)
            {
                DrawSettingSODetail(ref targetEditor, ref isTargetEditorFolded, settingSOProp);
            }
            else
            {
                EditorGUILayout.HelpBox("No Details", MessageType.Info);
            }

        }


        public static void DrawSettingSO(Type settingSOType, SerializedProperty settingSOProp)
        {
            EditorGUILayout.BeginHorizontal();
            //1,2 label and settingSO
            EditorGUILayout.PropertyField(settingSOProp);
            //3 create new
            if (GUILayout.Button("Create New",GUILayout.Width(100)))
            {
                var nextId = Resources.FindObjectsOfTypeAll(settingSOType).Length+1;
                var soPath = $"{PathTools.PATH_POWER_UTILITIES}/{settingSOType.Name}/_{settingSOType.Name}_{nextId}.asset";

                var newAsset = ScriptableObjectTools.CreateGetInstance(settingSOType,soPath);
                EditorGUIUtility.PingObject(newAsset);
                settingSOProp.objectReferenceValue = newAsset;
            }
            EditorGUILayout.EndHorizontal();
        }


        static void DrawSettingSODetail(ref Editor targetEditor, ref bool isTargetEditorFolded, SerializedProperty settingSOProp)
        {
            EditorTools.CreateEditor(settingSOProp.objectReferenceValue, ref targetEditor);

            isTargetEditorFolded = EditorGUILayout.Foldout(isTargetEditorFolded, settingSOProp.displayName, true);
            if (isTargetEditorFolded)
            {
                EditorGUI.indentLevel++;
                targetEditor.OnInspectorGUI();
                EditorGUI.indentLevel--;
            }
        }

        public static void DrawScriptScope(SerializedObject serializedObject)
        {
            var prop = serializedObject.FindProperty("m_Script");
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(prop, true);
            }
        }

        #endregion

        /// <summary>
        /// Save current labelWidth, then use labelWidth
        /// </summary>
        /// <param name="labelWidth"></param>
        public static void UpdateLabelWidth(float labelWidth)
        {
            lastLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = labelWidth;
        }

        /// <summary>
        /// Use last labelWidth 
        /// </summary>
        public static void RestoreLabelWidth()
        {
            EditorGUIUtility.labelWidth = lastLabelWidth;
        }

        /// <summary>
        /// currentViewWidth - indentWidth
        /// </summary>
        /// <returns></returns>
        public static float GetCurrentViewWidthWithIndent()
        {
            var viewWidth = EditorGUIUtility.currentViewWidth - 15 * EditorGUI.indentLevel;
            return viewWidth;
        }
    }
}
#endif