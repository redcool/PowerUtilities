#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Show editor window with a uxml
    /// </summary>
    public class CommonUXMLEditorWindow : BaseUXMLEditorWindow
    {
        public bool isShowCommonHeader = true;
        bool isCommonHeaderFolded;


        [MenuItem(ROOT_MENU + "/CommonUxmlWindow")]
        public static void OpenWindow()
        {
            var w = GetWindow<CommonUXMLEditorWindow>();
            w.titleContent = EditorGUITools.TempContent("CommonUxmlWindow");
            if(w.position.width< 1000) {
                w.position = new Rect(100, 100, 1000, 800);
            }
        }

        private void DrawHeader()
        {
            EditorGUI.BeginChangeCheck();
            isCommonHeaderFolded = EditorGUILayout.Foldout(isCommonHeaderFolded, EditorGUITools.TempContent("TreeAsset"), true);
            if (isCommonHeaderFolded)
            {
                EditorGUILayout.BeginHorizontal(EditorStylesEx.box);

                EditorGUILayout.LabelField("Uxml", GUILayout.Width(60));
                treeAsset = EditorGUILayout.ObjectField(treeAsset, typeof(VisualTreeAsset), true) as VisualTreeAsset;

                EditorGUILayout.LabelField("Event MonoScript", GUILayout.Width(60));
                eventMono = EditorGUILayout.ObjectField(eventMono, typeof(MonoScript), true) as MonoScript;
                var isValid = eventMono != null && eventMono.GetClass().IsImplementOf(typeof(IUIElementEvent));
                if (!isValid)
                {
                    eventMono = null;
                }

                EditorGUILayout.LabelField("Uss",GUILayout.Width(30));
                treeStyleSheet = EditorGUILayout.ObjectField(treeStyleSheet,typeof(StyleSheet), true) as StyleSheet;

                EditorGUILayout.EndHorizontal();
            }
            var isChanged = EditorGUI.EndChangeCheck();
            if (isChanged)
            {
                base.CreateGUI();
            }
        }
        public void OnGUI()
        {
            if (isShowCommonHeader)
                DrawHeader();
        }
        public void Update()
        {
            if (isShowCommonHeader)
                MoveTreeView();
        }


        private void MoveTreeView()
        {
            if (treeInstance == null)
                return;

            var pos = Vector2.zero;
            pos.y = 20;
            if (isCommonHeaderFolded)
            {
                pos.y += 20;
            }
            treeInstance.transform.position = pos;
        }
    }
}
#endif