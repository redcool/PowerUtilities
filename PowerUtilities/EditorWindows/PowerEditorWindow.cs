#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class PowerEditorWindow : EditorWindow
    {
        public bool isShowCommonHeader = true;

        public VisualTreeAsset treeAsset;
        VisualElement treeInstance;

        public MonoScript eventMono;
        public IUIElementEvent eventInstance;

        bool isCommonHeaderFolded;

        public const string ROOT_MENU = "PowerUtilities/Window";
        [MenuItem(ROOT_MENU + "/Common UxmlWindow")]
        public static void OpenWindow()
        {
            var w = GetWindow<PowerEditorWindow>();
            w.titleContent = EditorGUITools.TempContent("PowerEditorWin");
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

                EditorGUILayout.EndHorizontal();
            }
            var isChanged = EditorGUI.EndChangeCheck();
            if (isChanged)
            {
                CreateGUI();
                AddTreeEvents();
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

        /// <summary>
        /// create ui elements
        /// </summary>
        public void CreateGUI()
        {
            rootVisualElement.Clear();

            if (!treeAsset)
                return;

            treeInstance = treeAsset.CloneTree();
            rootVisualElement.Add(treeInstance);
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

        public void AddTreeEvents()
        {
            if (treeInstance == null)
                return;

            if(eventMono)
                eventInstance = Activator.CreateInstance(eventMono.GetClass()) as IUIElementEvent;

            if (eventInstance != null)
            {
                eventInstance.AddEvent(treeInstance);
            }

        }
    }
}
#endif